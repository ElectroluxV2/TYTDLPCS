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
            .Wrap(PythonManager.GetPackageBinary(PackageName))
            .WithArguments($"-v --add-header user-agent:Mozilla/5.0 -J {url}");
    }

    public Command DownloadContent(string url)
    {
        PythonManager.EnsurePackageExists(PackageName);

        return Cli
            .Wrap(PythonManager.GetPackageBinary(PackageName))
            .WithArguments($"--progress --add-header user-agent:Mozilla/5.0 -o - {url}");
    }

    public Command InstallOrUpgrade()
    {
        return PythonManager.InstalPackage(PackageName);
    }
}