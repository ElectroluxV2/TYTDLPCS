namespace TYTDLPCS;

static class BruteforceDownloader {


    static private List<IDownloader> Downloaders = new List<IDownloader>{new YtDlP()};

    static public void ExtremeDownload() {
        Downloaders.ForEach(downloader => downloader.Download());
    }
}