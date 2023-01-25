using Microsoft.AspNetCore.Mvc;
using TyranoKurwusBot.Filters;
using TyranoKurwusBot.Services;
using Telegram.Bot.Types;

namespace TyranoKurwusBot.Controllers;

public class BotController : ControllerBase
{
    private readonly ILogger<BotController> _logger;

    public BotController(ILogger<BotController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [ValidateTelegramBot]
    public IActionResult Post(
        [FromBody] Update update,
        [FromServices] IServiceScopeFactory serviceScopeFactory)
    {
        _logger.LogCritical("START");

        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var updateHandlers = scope.ServiceProvider.GetRequiredService<UpdateHandlers>();
            await updateHandlers.HandleUpdateAsync(update, new CancellationToken());
        });
        
        _logger.LogCritical("STOP");
        return new OkResult();
    }
}