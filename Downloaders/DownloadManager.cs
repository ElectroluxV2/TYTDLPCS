using CliWrap.EventStream;
using Microsoft.Extensions.Logging;
using TYTDLPCS.Logging;

namespace TYTDLPCS.Downloaders;

public static class DownloadManager
{
    private static readonly TimeSpan DefaultInstallTimeout = TimeSpan.FromMinutes(1);
    private static readonly ILogger Logger = LoggerManager.Factory.CreateLogger(typeof(DownloadManager));
    
    public static readonly IDownloader YtDlp = new YtDlp();

    public static async Task<bool> InstallOrUpdateAsync(this IDownloader downloader, CancellationToken? cancellationToken = null)
    {
        var downloaderFullName = downloader.GetType().FullName;
        Logger.LogInformation("Start of update of {DownloaderName} ", downloaderFullName);
        
        // If no token provided, we will kill installation after 1 minute, no matter of output
        var token = cancellationToken.GetValueOrDefault(new CancellationTokenSource(DefaultInstallTimeout).Token);
        
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
