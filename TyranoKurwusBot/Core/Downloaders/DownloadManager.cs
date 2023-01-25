using TyranoKurwusBot.Core.Logging;

namespace TyranoKurwusBot.Core.Downloaders;

public static partial class DownloadManager
{
    private static readonly TimeSpan ChildMaxTickTime = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan InstallCommandTimeout = TimeSpan.FromMinutes(5);
    private static readonly ILogger Logger = LoggerManager.Factory.CreateLogger(typeof(DownloadManager));
    public static readonly IDownloader YtDlp = new YtDlp();

    public static readonly IDownloader[] AvailableDownloaders = {YtDlp};
}
