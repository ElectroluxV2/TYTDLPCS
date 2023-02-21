using TyranoKurwusBot.Core.Downloaders;

namespace TyranoKurwusBot.Services;

public class PreparePackagesService : IHostedService
{
    private readonly ILogger<PreparePackagesService> _logger;

    public PreparePackagesService(ILogger<PreparePackagesService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var downloader in DownloadManager.AvailableDownloaders) await downloader.InstallOrUpdateAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}