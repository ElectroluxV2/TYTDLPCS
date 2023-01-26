using System.Text;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TyranoKurwusBot.Core.Common;

namespace TyranoKurwusBot.Services;

public class ConfigureWebhook
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BotConfiguration _botConfig;

    public ConfigureWebhook(
        ILogger<ConfigureWebhook> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> botOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _botConfig = botOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var pem = new char[5000];
        GenerateSslCertificate.Certificate.TryExportCertificatePem(pem, out var pemLength);

        var ms = new MemoryStream();

        await ms.WriteAsync(Encoding.GetEncoding(Encoding.UTF8.CodePage).GetBytes(pem).AsMemory(0, pemLength), cancellationToken);
        ms.Seek(0, SeekOrigin.Begin);

        // Configure custom endpoint per Telegram API recommendations:
        // https://core.telegram.org/bots/api#setwebhook
        // If you'd like to make sure that the webhook was set by you, you can specify secret data
        // in the parameter secret_token. If specified, the request will contain a header
        // "X-Telegram-Bot-Api-Secret-Token" with the secret token as content.
        var webhookAddress = $"{_botConfig.HostAddress}{_botConfig.Route}";
        _logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(
            url: webhookAddress,
            allowedUpdates: Array.Empty<UpdateType>(),
            secretToken: _botConfig.SecretToken,
            cancellationToken: cancellationToken,
            certificate: new InputFile(ms, "cert.pem")
        );

        var info = await botClient.GetWebhookInfoAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Has custom cert: {}, last error: {}, url: {}", info.HasCustomCertificate, info.LastErrorMessage, info.Url);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook on app shutdown
        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}