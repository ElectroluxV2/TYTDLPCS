using CliWrap;

namespace TYTDLPCS;

public interface IDownloader
{
    void Download(string url);

    Command InstallOrUpgrade();
}
