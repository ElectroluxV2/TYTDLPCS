using CliWrap;

namespace TyranoKurwusBot.Core.Downloaders;

public interface IDownloader
{
    Command DownloadMetadata(string url);
    Command DownloadContent(string url);

    Command InstallOrUpgrade();
}