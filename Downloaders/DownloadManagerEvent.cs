namespace TYTDLPCS.Downloaders;

public abstract record DownloadManagerEvent;

public record MetadataError(
    string Url,
    string Message,
    string DownloaderFullName,
    int ExitCode,
    DateTimeOffset ExitTime,
    TimeSpan RunTime,
    DateTimeOffset StartTime
) : DownloadManagerEvent;


public record MetadataContent(
    string Url,
    string Json
) : DownloadManagerEvent;
