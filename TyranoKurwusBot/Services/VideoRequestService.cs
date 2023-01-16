using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TyranoKurwusBot.Core.Downloaders;

namespace TyranoKurwusBot.Services;

public partial class VideoRequestService 
{
    private readonly ILogger<VideoRequestService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly Regex _extractUrlRegex = MyRegex();
    
    [GeneratedRegex("https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)")]
    private static partial Regex MyRegex();


    public VideoRequestService(ILogger<VideoRequestService> logger, ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task Process(Message message, CancellationToken cancelationToken)
    {
        _logger.LogInformation("Got message: {}", message.Text);

        var links = _extractUrlRegex.Matches(message.Text ?? "")
            .Select(m => m.Value) 
            .ToArray();

        foreach (var link in links)
        {
            await Handle(link, message, cancelationToken);
        }
    }

    private async Task Handle(string link, Message message, CancellationToken cancellationToken)
    {
        await foreach (var downloadManagerEvent in DownloadManager.YtDlp.DownloadAsync(link).WithCancellation(cancellationToken))
            switch (downloadManagerEvent)
            {
                case MetadataError metadataError:
                    _logger.LogError("Failed to download metadata for {Url}: {Message}", metadataError.Url, metadataError.Message);
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Error: {metadataError.Message.Replace(".", "\\.")}",
                        // parseMode: ParseMode.MarkdownV2,
                        disableNotification: true,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken
                    );
                    
                    return;

                case MetadataSuccess metadataSuccess:
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Download: {metadataSuccess.Metadata.Title.Replace(".", "\\.")}",
                        // parseMode: ParseMode.MarkdownV2,
                        disableNotification: true,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken
                    );
                    
                    break;
                
                // case ContentBytes contentBytes:
                //     
                //     await _botClient.SendVideoAsync(
                //         chatId: message.Chat.Id,
                //         video: new InputFile(),
                //         thumb: "https://raw.githubusercontent.com/TelegramBots/book/master/src/2/docs/thumb-clock.jpg",
                //         supportsStreaming: true,
                //         cancellationToken: cancellationToken);
                //     
                //     break;

                // case ContentBegin contentBegin:
                //     logger.LogInformation("Saving {}", contentBegin.Metadata.Title);
                //     file = File.Create(contentBegin.Metadata.Id + ".mp4");
                //     file.Seek(0, SeekOrigin.Begin);
                //     break;
                //
                // case ContentBytes contentBytes:
                //     logger.LogCritical("L: {}", contentBytes.Bytes.Length);
                //     file!.Write(contentBytes.Bytes);
                //     break;
                //
                // case ContentEnd contentEnd:
                //     logger.LogInformation("Saved to {File}", file!.Name);
                //     file!.Close();
                //     break;
            }
    }
}