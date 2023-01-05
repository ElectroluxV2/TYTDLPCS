using Microsoft.Extensions.Logging;
using TYTDLPCS.Logging;

namespace TYTDLPCS.Downloaders;

public static partial class DownloadManager
{
    private static readonly TimeSpan ChildMaxTickTime = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan InstallCommandTimeout = TimeSpan.FromMinutes(1);
    private static readonly ILogger Logger = LoggerManager.Factory.CreateLogger(typeof(DownloadManager));
    public static readonly IDownloader YtDlp = new YtDlp();
}
