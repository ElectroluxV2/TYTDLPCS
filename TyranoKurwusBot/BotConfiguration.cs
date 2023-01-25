namespace TyranoKurwusBot;

public class BotConfiguration
{
    public static readonly string Configuration = "TelegramBotConfiguration";

    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string BaseTelegramUrl { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string SecretToken { get; init; } = default!;
}
