using Microsoft.AspNetCore.Mvc;
using TyranoKurwusBot.Filters;
using TyranoKurwusBot.Services;
using Telegram.Bot.Types;

namespace TyranoKurwusBot.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public IActionResult Post(
        [FromBody] Update update,
        [FromServices] IServiceScopeFactory serviceScopeFactory)
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var updateHandlers = scope.ServiceProvider.GetRequiredService<UpdateHandlers>();
            await updateHandlers.HandleUpdateAsync(update);
        });

        return new OkResult();
    }
}