namespace TYTDLPCS.Downloaders;

public abstract record DownloadManagerEvent;

public record MetadataError(
    string Url,
    string DownloaderFullName,
    string Message
) : DownloadManagerEvent;

public record MetadataSuccess(
    VideoMetdata Metadata
) : DownloadManagerEvent;
