namespace TYTDLPCS;

using static Python.PythonManager;

class YtDlP : IDownloader
{
    void IDownloader.Download(string url)
    {
        // var download = CreateCliProcess("yt-dlp", $"\"{url}\"");
        // download.Start();
        // download.WaitForExit();
    }

    void IDownloader.Update()
    {
        // PipInstall("youtube-dl");
    }
}

