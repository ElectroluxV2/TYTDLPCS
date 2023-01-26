using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TyranoKurwusBot.Core.Downloaders;
using TyranoKurwusBot.Extensions;

namespace TyranoKurwusBot.Services;

public partial class VideoRequestService 
{
    private readonly Regex _extractUrlRegex = UrlRegex();
    private readonly ILogger<VideoRequestService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly TelegramPushBot _telegramPushBot;

    [GeneratedRegex("https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)")]
    private static partial Regex UrlRegex();

    public VideoRequestService(ILogger<VideoRequestService> logger, ITelegramBotClient botClient, TelegramPushBot telegramPushBot)
    {
        _logger = logger;
        _botClient = botClient;
        _telegramPushBot = telegramPushBot;
    }

    public async Task Process(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parsing message for links: {}", message.Text);

        var links = _extractUrlRegex.Matches(message.Text ?? "")
            .Select(m => m.Value) 
            .ToArray();

        foreach (var link in links)
        {
            await Handle(link, message, cancellationToken);
        }
    }

    private async Task Handle(string link, Message message, CancellationToken cancellationToken)
    {
        var metadataEvent = await DownloadManager.YtDlp.DownloadMetadataAsync(link, cancellationToken);

        if (metadataEvent is MetadataError metadataError)
        {
            _logger.LogError("Failed to download metadata for {Url}: {Message}", metadataError.Url, metadataError.Message);
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Error: {metadataError.Message.Replace(".", "\\.")}",
                disableNotification: true,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );
                    
            return;
        }

        var metadataSuccess = (metadataEvent as MetadataSuccess)!;
        var dowloadMessage = await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Download: {metadataSuccess.Metadata.Title.Replace(".", "\\.")}",
            disableNotification: true,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken
        );
        
        await _telegramPushBot.PushVideo(message.Chat.Id, message.From!.Username ?? message.From!.FirstName, metadataSuccess, cancellationToken, async (stream, httpContent, transportContext) =>
        {
            await foreach (var downloadManagerEvent in DownloadManager.YtDlp.MakeContentAsyncIterator(metadataSuccess.Metadata, cancellationToken).WithCancellation(cancellationToken))
                switch (downloadManagerEvent)
                {
                    case ContentBegin contentBegin:
                        _logger.LogInformation("Pushing {}", contentBegin.Metadata.Title);
                        break;
                    
                    case ContentBytes contentBytes:
                        stream.Write(contentBytes.Bytes);
                        break;
                    
                    case ContentEnd contentEnd:
                        _logger.LogInformation("Pushed");
                        stream.Close();
                        break;
                }
            
            // Delete redundant messages
            await _botClient.DeleteMessageAsync(dowloadMessage.Chat.Id, dowloadMessage.MessageId, cancellationToken);
            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
        });
    }
}