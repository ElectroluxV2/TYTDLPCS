using Microsoft.Extensions.Logging;
using TYTDLPCS.Downloaders;
using TYTDLPCS.Logging;

var logger = LoggerManager.Factory.CreateLogger(typeof(Program));

await foreach (var downloadManagerEvent in DownloadManager.YtDlp.DownloadAsync("https://www.youtube.com/playlist?list=PLJQIrLkntvSpYpnf__nypuEHyei-u7L3w"))
    switch (downloadManagerEvent)
    {
        case MetadataError metadataError:
            logger.LogError("Failed to download metadata for {Url}. Error: {Message}", metadataError.Url, metadataError.Message);
            break;
        case MetadataSuccess metadataSuccess:
            // logger.LogInformation("Downloaded metadata for {Url}, Length {Length}", metadataSuccess.Url, metadataSuccess.Json.Length);
            Console.WriteLine(metadataSuccess.Metdata.Url);
            
            foreach (var metdataEntry in metadataSuccess.Metdata.Entries)
            {
                Console.WriteLine(metdataEntry?.Url);
            }
            
            break;
    }