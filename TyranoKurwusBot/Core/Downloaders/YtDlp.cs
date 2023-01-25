using CliWrap;
using PythonManager = TyranoKurwusBot.Core.Python.PythonManager;

namespace TyranoKurwusBot.Core.Downloaders;

public sealed class YtDlp : IDownloader
{
    private const string PackageName = "yt-dlp";

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
            .WithArguments($"--progress --recode-video mp4 --add-header user-agent:Mozilla/5.0 -o - {url}");
    }

    public Command InstallOrUpgrade()
    {
        return PythonManager.InstalPackage(PackageName);
    }
}