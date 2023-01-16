using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TyranoKurwusBot.Services;

public class UpdateHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly VideoRequestService _videoRequestService;
    private readonly ILogger<UpdateHandlers> _logger;

    public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, VideoRequestService videoRequestService)
    {
        _botClient = botClient;
        _logger = logger;
        _videoRequestService = videoRequestService;
    }

    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)

    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => _videoRequestService.Process(message, cancellationToken),
            { EditedMessage: { } message } => _videoRequestService.Process(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken) 
    {
        _logger.LogWarning("Unknown update type", update.Type.ToString());

        return Task.CompletedTask;
    }
}
