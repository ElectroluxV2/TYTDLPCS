using CliWrap;
using TYTDLPCS.Python;

namespace TYTDLPCS.Downloaders;

public sealed class YtDlp : IDownloader
{
    private const string PackageName = "yt-dlp";

    public Command DownloadMetadata(string url)
    {
        return Cli
            .Wrap(PythonManager.GetPackageBinary(PackageName))
            .WithArguments($"-v --add-header user-agent:Mozilla/5.0 -J {url}");
    }

    public Command DownloadContent(string url)
    {
        return Cli
            .Wrap(PythonManager.GetPackageBinary(PackageName))
            .WithArguments($"--progress --add-header user-agent:Mozilla/5.0 -o - {url}");
    }

    public Command InstallOrUpgrade()
    {
        return PythonManager.InstalPackage(PackageName);
    }
}