using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TyranoKurwusBot.Core.Downloaders;
using File = System.IO.File;

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
        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Download: {metadataSuccess.Metadata.Title.Replace(".", "\\.")}",
            disableNotification: true,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken
        );

        using var multipartFormDataContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        var pushStreamContent = new PushStreamContent(async (stream, httpContent, transportContext) =>
        {
            await foreach (var downloadManagerEvent in DownloadManager.YtDlp.MakeContentAsyncIterator(metadataSuccess.Metadata, cancellationToken).WithCancellation(cancellationToken))
                switch (downloadManagerEvent)
                {
                
                    case ContentBegin contentBegin:
                        _logger.LogInformation("Saving {}", contentBegin.Metadata.Title);
                        break;
                    
                    case ContentBytes contentBytes:
                        _logger.LogCritical("L: {}", contentBytes.Bytes.Length);
                        stream.Write(contentBytes.Bytes);
                        break;
                    
                    case ContentEnd contentEnd:
                        _logger.LogInformation("Saved");
                        stream.Close();
                        break;
                }
        });

        pushStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        multipartFormDataContent.Add(pushStreamContent, "video", "test.mp4");
        
        var httpClient = new HttpClient();
        var endpoint = $"https://api.telegram.org/bot<token>/sendVideo?supports_streaming=true&chat_id={message.Chat.Id}";
        
        _logger.LogWarning("Sending: {}", endpoint);
        
        var response = await httpClient.PostAsync(endpoint, multipartFormDataContent, cancellationToken);
        httpClient.Dispose();
        var sd = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Response: {}, {}",response.StatusCode,  sd);
    }
}