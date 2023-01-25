using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Telegram.Bot;
using TyranoKurwusBot.Core.Downloaders;

namespace TyranoKurwusBot.Extensions;

public class TelegramPushBot
{
    private readonly ILogger<TelegramPushBot> _logger;
    private readonly TelegramBotClientOptions _options;

    public TelegramPushBot(TelegramBotClientOptions options, ILogger<TelegramPushBot> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task PushVideo(long chatId, MetadataSuccess metadataSuccess, CancellationToken cancellationToken,
        Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable)
    {
        using var multipartFormDataContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        var pushStreamContent = new PushStreamContent(onStreamAvailable);

        pushStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        multipartFormDataContent.Add(pushStreamContent, "video", "video.mp4");
        
        var httpClient = new HttpClient();
        var endpoint = $"{_options.BaseUrl}/bot{_options.Token}/sendVideo?supports_streaming=true&chat_id={chatId}&disable_notification=true&caption={metadataSuccess.Metadata.Title}";

        _logger.LogInformation("Pushing video to Telegram: {}", endpoint);
        var response = await httpClient.PostAsync(endpoint, multipartFormDataContent, cancellationToken);
        httpClient.Dispose();
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Got response response: {}, {}",response.StatusCode,  responseContent);
    }
}