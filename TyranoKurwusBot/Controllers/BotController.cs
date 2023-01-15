using Microsoft.AspNetCore.Mvc;
using TyranoKurwusBot.Filters;
using TyranoKurwusBot.Services;
using Telegram.Bot.Types;

namespace TyranoKurwusBot.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandlers handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}