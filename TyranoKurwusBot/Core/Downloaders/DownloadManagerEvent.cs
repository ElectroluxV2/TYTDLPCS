namespace TyranoKurwusBot.Core.Downloaders;

public abstract record DownloadManagerEvent;

public record MetadataError(
    string Url,
    string DownloaderFullName,
    string Message
) : DownloadManagerEvent;

public record MetadataSuccess(
    VideoMetadata Metadata
) : DownloadManagerEvent;

public record ContentBytes(
    byte[] Bytes,
    VideoMetadata Metadata
) : DownloadManagerEvent;

public record ContentError(
    VideoMetadata Metadata
) : DownloadManagerEvent;

public record ContentBegin(
    VideoMetadata Metadata
) : DownloadManagerEvent;

public record ContentEnd(
    VideoMetadata Metadata
) : DownloadManagerEvent;
