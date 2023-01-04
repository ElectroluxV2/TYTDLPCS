using CliWrap;

namespace TYTDLPCS;

public interface IDownloader
{
    Command DownloadMetadata(string url);
    Command DownloadContent(string url);

    Command InstallOrUpgrade();
}
