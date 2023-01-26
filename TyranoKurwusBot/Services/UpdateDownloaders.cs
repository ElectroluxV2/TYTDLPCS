using TyranoKurwusBot.Core.Downloaders;

namespace TyranoKurwusBot.Services;

public class UpdateDownloaders : BackgroundService
{
    private readonly ILogger<UpdateDownloaders> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public UpdateDownloaders(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateDownloaders> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Updating all updaters now");
        await ExecuteUpdate();
        _logger.LogInformation("Complete, next update in 1 day");
        
        var timer = new PeriodicTimer(TimeSpan.FromDays(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteUpdate();
        }
    }

    private static async Task ExecuteUpdate()
    {
        var tasks = DownloadManager.AvailableDownloaders.Select(downloader => downloader.InstallOrUpdateAsync());
        await Task.WhenAll(tasks);
    }
}