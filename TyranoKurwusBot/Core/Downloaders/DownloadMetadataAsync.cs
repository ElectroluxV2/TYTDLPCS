using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.EventStream;
using TyranoKurwusBot.Core.Common;

namespace TyranoKurwusBot.Core.Downloaders;

public static partial class DownloadManager
{
    public static async Task<DownloadManagerEvent> DownloadMetadataAsync(this IDownloader downloader, string url, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName!;
        Logger.LogInformation("Fetching metadata for {Url} using {DownloaderName} ", url, downloaderFullName);

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        var command = downloader.DownloadMetadata(url).WithValidation(CommandResultValidation.None);

        var watchdogTokenSource = new CancellationTokenSource();
        var (watchdog, tick) = TickBasedWatchDog.Make(ChildMaxTickTime, cancellationTokenSource, watchdogTokenSource.Token);

        string? error = null;
        try
        {
            await foreach (var commandEvent in command.ListenAsync(Encoding.UTF8, Encoding.UTF8, cancellationTokenSource.Token, cancellationTokenSource.Token))
            {
                tick();

                switch (commandEvent)
                {
                    case StartedCommandEvent startedCommand:
                        Logger.LogInformation("Spawned child process for {DownloaderName}; PID: {StartedProcessId}",
                            downloaderFullName, startedCommand.ProcessId);
                        break;
                    case StandardOutputCommandEvent stdOut:
                        await streamWriter.WriteLineAsync(stdOut.Text);
                        break;
                    case StandardErrorCommandEvent stdErr:
                        var log = stdErr.Text;

                        const string prefix = "[download] ";
                        if (!log.StartsWith(prefix))
                        {
                            Logger.LogDebug("STDERR[{DownloaderName}] {StdErrText}", downloaderFullName, stdErr.Text);
                            break;
                        }
                        
                        Logger.LogInformation("STDERR[{DownloaderName}] {Log}", downloaderFullName, log[prefix.Length..]);
                        break;
                    case ExitedCommandEvent exited:
                        Logger.LogInformation("Child process for {DownloaderName} exited with code {Code}",
                            downloaderFullName, exited.ExitCode);
                        break;
                }
            }
        }
        catch
        {
            error = $"{downloaderFullName} failed to report metadata download progress in {ChildMaxTickTime.ToString()}.";
        }
        
        watchdogTokenSource.Cancel();
        watchdog.Join();

        if (error is not null)
        {
             return new MetadataError(
                url,
                downloaderFullName,
                error
            );
        }

        await streamWriter.FlushAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        VideoMetadata? metadata = null;

        try
        {
            metadata = await JsonSerializer.DeserializeAsync<VideoMetadata>(memoryStream, cancellationToken: cancellationTokenSource.Token, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }
        catch
        {
            // ignored
        }

        if (metadata is null)
        {
            return new MetadataError(
                url,
                downloaderFullName,
                "Downloader error TODO: implement error message"
            );
        }

        return new MetadataSuccess(metadata);
    }
}