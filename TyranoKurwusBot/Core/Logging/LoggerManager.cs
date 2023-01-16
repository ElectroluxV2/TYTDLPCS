namespace TyranoKurwusBot.Core.Logging;

public static class LoggerManager
{
    public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddConsole();
    });
}