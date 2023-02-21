using CliWrap;
using TyranoKurwusBot.Core.Python;

namespace TyranoKurwusBot.Core.Downloaders;

public sealed class YtDlp : IDownloader
{
    public string PackageName => "yt-dlp";

    public Command DownloadMetadata(string url)
    {
        PythonManager.EnsurePackageExists(PackageName);

        return Cli
            .Wrap(PackageName)
            .WithArguments($"-v -J {url}");
    }

    public Command DownloadContent(string url)
    {
        PythonManager.EnsurePackageExists(PackageName);

        return Cli
            .Wrap(PackageName)
            .WithArguments($"--progress --recode-video mp4 -o - {url}");
    }

    public Command InstallOrUpgrade()
    {
        return PythonManager.InstallPackage(PackageName);
    }
}