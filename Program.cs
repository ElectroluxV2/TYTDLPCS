using Microsoft.Extensions.Logging;
using TYTDLPCS.Downloaders;
using TYTDLPCS.Logging;

var logger = LoggerManager.Factory.CreateLogger(typeof(Program));

await foreach (var downloadManagerEvent in DownloadManager.YtDlp.DownloadAsync("https://www.youtube.com/playlist?list=PLJQIrLkntvSpYpnf__nypuEHyei-u7L3w"))
    switch (downloadManagerEvent)
    {
        case MetadataError metadataError:
            logger.LogError("Failed to download metadata for {Url}: {Message}", metadataError.Url, metadataError.Message);
            break;
        case MetadataSuccess metadataSuccess:
            logger.LogInformation("{}", metadataSuccess.Metadata.Url);

            if (metadataSuccess.Metadata.Entries is null)
            {
                continue; // Not a playlist
            }

            foreach (var metadataEntry in metadataSuccess.Metadata.Entries)
                logger.LogInformation("{}", metadataEntry?.Url);

            break;
    }