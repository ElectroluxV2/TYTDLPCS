namespace TYTDLPCS;

interface IDownloader {
    abstract void Download(string url);

    abstract void Update();
}
