using TyranoKurwusBot.Core.Downloaders;

namespace TyranoKurwusBot.Services;

public class UpdateDownloaders : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public UpdateDownloaders(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var downloader in DownloadManager.AvailableDownloders)
            {
                await downloader.InstallOrUpdateAsync();
            }
        }
    }
}