using System.Text;
using CliWrap;
using CliWrap.EventStream;
using TyranoKurwusBot.Core.Common;

namespace TyranoKurwusBot.Core.Downloaders;

public static partial class DownloadManager
{
    public static async IAsyncEnumerable<DownloadManagerEvent> MakeContentAsyncIterator(this IDownloader downloader, VideoMetadata metadata, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName!;
        Logger.LogInformation("Fetching content for {Url} using {DownloaderName} ", metadata.Url, downloaderFullName);

        using var memoryStream = new MemoryStream();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());
        var command = downloader
            .DownloadContent(metadata.Url)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream));

        var watchdogTokenSource = new CancellationTokenSource();
        var (watchdog, tick) = TickBasedWatchDog.Make(ChildMaxTickTime, cancellationTokenSource, watchdogTokenSource.Token);

        await foreach (var commandEvent in command.ListenBytesAsync(1024 * 1024, Encoding.UTF8, Encoding.UTF8, cancellationTokenSource.Token, cancellationTokenSource.Token))
        {
            tick();

            switch (commandEvent)
            {
                case StartedCommandEvent startedCommand:
                    Logger.LogInformation("Spawned child process for {DownloaderName}; PID: {StartedProcessId}",
                        downloaderFullName, startedCommand.ProcessId);
                        yield return new ContentBegin(metadata);
                    break;

                case StandardBinaryOutputCommandEvent stdOut:
                    yield return new ContentBytes(stdOut.Bytes, metadata);
                    break;
                    
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