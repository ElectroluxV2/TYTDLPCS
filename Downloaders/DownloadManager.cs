using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.EventStream;
using Microsoft.Extensions.Logging;
using TYTDLPCS.Logging;

namespace TYTDLPCS.Downloaders;

public static class DownloadManager // TODO: Split manager to partial class
{
    private static readonly TimeSpan DefaultCommandTimeout = TimeSpan.FromMinutes(1);
    private static readonly ILogger Logger = LoggerManager.Factory.CreateLogger(typeof(DownloadManager));

    public static readonly IDownloader YtDlp = new YtDlp();

    public static async IAsyncEnumerable<DownloadManagerEvent> DownloadAsync(this IDownloader downloader, string url, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName!;
        Logger.LogInformation("Fetching metadata for {Url} using {DownloaderName} ", url, downloaderFullName);

        // If no token provided, we will kill installation after 1 minute no matter of output
        var token = cancellationToken.GetValueOrDefault(new CancellationTokenSource(DefaultCommandTimeout).Token);

        var downloadMetadataCommand = downloader.DownloadMetadata(url);

        var stdOutBuffer = new MemoryStream();
        var stdErrBuffer = new StringBuilder();
        var jsonCommandResult = await (downloadMetadataCommand
            // .WithStandardOutputPipe(PipeTarget.ToStream(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None) | stdOutBuffer)
            .ExecuteAsync(token);

        stdOutBuffer.Position = 0; /*reset Position to start*/
        VideoMetdata? metadata = null;
        
        try
        {
            metadata = (await JsonSerializer.DeserializeAsync<VideoMetdata>(stdOutBuffer, cancellationToken: token, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }))!;

            if (metadata is null) throw new Exception();
        }
        catch
        {
            // ignored
        }

        if (metadata is null)
        {
            yield return new MetadataError(
                url,
                stdErrBuffer.ToString(),
                downloaderFullName,
                jsonCommandResult.ExitCode,
                jsonCommandResult.ExitTime,
                jsonCommandResult.RunTime,
                jsonCommandResult.StartTime
            );
            yield break;
        }


        // Logger.LogWarning("Metadata error: {MetaDataError}", stdErrBuffer.ToString());

        // Logger.LogInformation("Metadata response: {MetaData}", json);

        

        yield return new MetadataSuccess(metadata);
        // Jak sie wyjebalo to yield event error

        // Jak nie to event start z metadata

        // Eventy z binary data

        // Event z koniec
    }

    public static async Task<bool> InstallOrUpdateAsync(this IDownloader downloader, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName;
        Logger.LogInformation("Start of update of {DownloaderName} ", downloaderFullName);

        // If no token provided, we will kill installation after 1 minute, no matter of output
        var token = cancellationToken.GetValueOrDefault(new CancellationTokenSource(DefaultCommandTimeout).Token);

        var installCommand = downloader.InstallOrUpgrade();

        var success = false;
        try
        {
            await foreach (var commandEvent in installCommand.ListenAsync(token))
            {
                switch (commandEvent)
                {
                    case StartedCommandEvent startedCommand:
                        Logger.LogInformation("Spawned child process for {DownloaderName}; PID: {StartedProcessId}",
                            downloaderFullName, startedCommand.ProcessId);
                        break;
                    case StandardOutputCommandEvent stdOut:
                        Logger.LogInformation("STDOUT[{DownloaderName}] {StdOutText}", downloaderFullName, stdOut.Text);
                        break;
                    case StandardErrorCommandEvent stdErr:
                        Logger.LogWarning("STDERR[{DownloaderName}] {StdErrText}", downloaderFullName, stdErr.Text);
                        break;
                    case ExitedCommandEvent exited:
                        Logger.LogInformation("Child process for {DownloaderName} exited with code {Code}",
                            downloaderFullName, exited.ExitCode);
                        break;
                }

                if (commandEvent is not ExitedCommandEvent exitedCommandEvent) continue;

                success = exitedCommandEvent.ExitCode == 0;
            }
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Failed to install {DownloaderName}", downloaderFullName);
        }

        return success;
    }
}