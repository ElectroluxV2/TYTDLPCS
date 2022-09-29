namespace TYTDLPCS;

using static Python.PythonManager;

class YtDlP : IDownloader
{
    void IDownloader.Download()
    {
        var download = CreateCliProcess("yt-dlp", "https://www.youtube.com/watch?v=tPEE9ZwTmy0");
        download.Start();
        download.WaitForExit();
    }

    void IDownloader.Update()
    {
        PipInstall("youtube-dl");
    }
}

