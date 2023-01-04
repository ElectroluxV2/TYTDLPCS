using Microsoft.Extensions.Logging;
using TYTDLPCS.Downloaders;
using TYTDLPCS.Logging;

var logger = LoggerManager.Factory.CreateLogger(typeof(Program));

await foreach (var downloadManagerEvent in DownloadManager.YtDlp.DownloadAsync("https://www.youhtube.com/watch?v=tPEE9ZwTmy0"))
{
    switch (downloadManagerEvent)
    {
        case MetadataError metadataError:
            logger.LogError("Failed to download metadata for {Url}. Error: {Message}", metadataError.Url, metadataError.Message);
            break;
        case MetadataContent metadataContent:
            logger.LogInformation("Downloaded metadata for {Url}, Length {Length}", metadataContent.Url, metadataContent.Json.Length);
            break;
    }
}
