using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.EventStream;
using Microsoft.Extensions.Logging;
using TYTDLPCS.Common;

namespace TYTDLPCS.Downloaders;

public static partial class DownloadManager
{
    public static async IAsyncEnumerable<DownloadManagerEvent> DownloadAsync(this IDownloader downloader, string url, CancellationToken? cancellationToken = null)
    {
        var metadataEvent = await downloader.HandleMetadata(url);

        yield return metadataEvent;

        if (metadataEvent is not MetadataSuccess metadataSuccessEvent)
        {
            yield break;
        }

        await foreach (var downloadManagerEvent in downloader.HandleContent(metadataSuccessEvent.Metadata)) yield return downloadManagerEvent;
    }
    
    private static async Task<DownloadManagerEvent> HandleMetadata(this IDownloader downloader, string url, CancellationToken? cancellationToken = null)
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

    private static async IAsyncEnumerable<DownloadManagerEvent> HandleContent(this IDownloader downloader, VideoMetadata metadata, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName!;
        Logger.LogInformation("Fetching content for {Url} using {DownloaderName} ", metadata.Url, downloaderFullName);

        var memoryStream = new MemoryStream();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());
        var command = downloader
            .DownloadContent(metadata.Url)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream));

        var watchdogTokenSource = new CancellationTokenSource();
        var (watchdog, tick) = TickBasedWatchDog.Make(ChildMaxTickTime, cancellationTokenSource, watchdogTokenSource.Token);

        await foreach (var commandEvent in command.ListenAsync(Encoding.UTF8, Encoding.UTF8, cancellationTokenSource.Token, cancellationTokenSource.Token))
        {
            tick();

            switch (commandEvent)
            {
                case StartedCommandEvent startedCommand:
                    Logger.LogInformation("Spawned child process for {DownloaderName}; PID: {StartedProcessId}",
                        downloaderFullName, startedCommand.ProcessId);
                        yield return new ContentBegin(metadata);
                    break;
                case StandardOutputCommandEvent stdOut:
                {
                    // Actually we should check if this library is capable of passing raw bytes as event param instead of this hack
                    var memoryStreamBuffer = memoryStream.GetBuffer();

                    // We would like to pass std out bytes as separate independent chunks
                    var chunk = memoryStreamBuffer.ToArray();
                    // But only if there are any bytes, this event occurs more often than actual data push (MemoryStream has internal buffer)
                    if (chunk.Length > 0)
                        yield return new ContentBytes(chunk, metadata);
                    
                    // Clear memory stream buffer and reset it back to start
                    Array.Clear(memoryStreamBuffer, 0, memoryStreamBuffer.Length);
                    memoryStream.Position = 0;
                    memoryStream.SetLength(0);
                    memoryStream.Capacity = 0;

                    break;
                }
                    
                case StandardErrorCommandEvent stdErr:
                    Logger.LogInformation("STDERR[{DownloaderName}] {Log}", downloaderFullName, stdErr.Text);
                    break;
                case ExitedCommandEvent exited:
                    Logger.LogInformation("Child process for {DownloaderName} exited with code {Code}",
                        downloaderFullName, exited.ExitCode);
                    break;
            }
        }

        watchdogTokenSource.Cancel();
        watchdog.Join();

        yield return new ContentEnd(metadata);
    }
}