using CliWrap;

namespace TyranoKurwusBot.Core.Downloaders;

public interface IDownloader
{
    public string PackageName { get; }
    Command DownloadMetadata(string url);
    Command DownloadContent(string url);

    Command InstallOrUpgrade();
}