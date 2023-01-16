using Microsoft.Extensions.Logging;
using TYTDLPCS.Downloaders;
using TYTDLPCS.Logging;

var logger = LoggerManager.Factory.CreateLogger(typeof(Program));
var url = "https://www.youtube.com/watch?v=tPEE9ZwTmy0"; // 1s
url = "https://www.youtube.com/watch?v=fQtEVhOfKAA"; // 8s
url = "https://www.youtube.com/watch?v=kFMZUxX6K6o"; // 4m15s
url = "https://www.youtube.com/watch?v=bF8BP7RJ8Dc";
url = "https://www.youtube.com/watch?v=RaBpyvKjQjs";

FileStream? file = null;

await foreach (var downloadManagerEvent in DownloadManager.YtDlp.DownloadAsync(url))
    switch (downloadManagerEvent)
    {
        case MetadataError metadataError:
            logger.LogError("Failed to download metadata for {Url}: {Message}", metadataError.Url, metadataError.Message); 
            break;

        case MetadataSuccess metadataSuccess:
            logger.LogInformation("Url: {}", metadataSuccess.Metadata.Url);
            break;

        case ContentBegin contentBegin:
            logger.LogInformation("Saving {}", contentBegin.Metadata.Title);
            file = File.Create(contentBegin.Metadata.Id + ".mp4");
            file.Seek(0, SeekOrigin.Begin);
            break;

        case ContentBytes contentBytes:
            logger.LogCritical("L: {}", contentBytes.Bytes.Length);
            file!.Write(contentBytes.Bytes);
            break;

        case ContentEnd contentEnd:
            logger.LogInformation("Saved to {File}", file!.Name);
            file!.Close();
            break;
    }
