using CliWrap;
using TYTDLPCS.Python;

namespace TYTDLPCS.Downloaders;

public sealed class YtDlp : IDownloader
{
    private const string PackageName = "yt-dlp";
    
    public void Download(string url)
    {
        throw new NotImplementedException();
    }

    public Command InstallOrUpgrade()
    {
        return PythonManager.InstalPackage(PackageName);
    }
}