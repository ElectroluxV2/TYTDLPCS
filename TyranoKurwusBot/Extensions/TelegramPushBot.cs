using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
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

    public async Task PushVideo(long chatId, string username, MetadataSuccess metadataSuccess,
        CancellationToken cancellationToken,
        Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable)
    {
        using var multipartFormDataContent = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        var pushStreamContent = new PushStreamContent(onStreamAvailable);

        pushStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        multipartFormDataContent.Add(pushStreamContent, "video", "video.mp4");
        
        var httpClient = new HttpClient();
        var parameters = new Dictionary<string, string>
        {
            {"supports_streaming", "true"},
            {"disable_notification", "true"},
            {"allow_sending_without_reply", "true"},
            {"width", metadataSuccess.Metadata.Width.ToString()},
            {"height", metadataSuccess.Metadata.Height.ToString()},
            {"chat_id", chatId.ToString()},
            {"caption", $"{metadataSuccess.Metadata.Title}\n\nRequested by {username}."},
        };

        var endpoint = $"{_options.BaseUrl}/bot{_options.Token}/sendVideo?{QueryString.Create(parameters!).ToString()}";
        _logger.LogInformation("Pushing video to Telegram: {}", endpoint);
        var response = await httpClient.PostAsync(endpoint, multipartFormDataContent, cancellationToken);
        httpClient.Dispose();
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Got response response: {}, {}",response.StatusCode,  responseContent);
    }
}