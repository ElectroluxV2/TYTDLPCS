using System.Collections.Concurrent;
using System.Reflection;
using System.Security;
using System.Text.Json;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace TyranoKurwusBot.Services;

public class AllowedUsers : IHostedService
{
    private readonly string _password;
    private readonly ILogger<AllowedUsers> _logger;
    private static readonly string StorageFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "..", "..", "..", "allowed-users.json");
    private readonly ConcurrentDictionary<long, bool> _allowedUsersIds = new();

    public AllowedUsers(ILogger<AllowedUsers> logger, IConfiguration configuration)
    {
        _logger = logger;
        _password = configuration.GetValue<string>("TelegramBotConfiguration:SecretToken") ?? throw new SecurityException("TelegramBotConfiguration:SecretToken is empty");
    }

    public bool IsAllowed(Update update)
    {
        var userId = update.Message?.From?.Id;

        if (userId is null)
        {
            _logger.LogWarning("Missing user id");
            return false;
        }


        if (_allowedUsersIds.ContainsKey(userId.Value))
        {
            return true;
        }
        
        var contents = update.Message?.Text ?? update.EditedMessage?.Text ?? "";

        if (contents.Trim() == _password.Trim())
        {
            return _allowedUsersIds.TryAdd(userId.Value, true);
        }

        _logger.LogWarning("User with id: {} tried to access bot with: {}.", userId, contents);
        return false;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var reader = File.OpenText(StorageFile);
            var deserialized = await JsonSerializer.DeserializeAsync<Dictionary<long, bool>>(reader.BaseStream, cancellationToken: cancellationToken);
            foreach (var (key, value) in deserialized!)
            {
                _allowedUsersIds.TryAdd(key, value);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to read: {}, try to restart app", StorageFile);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try {
            await using var writer = File.CreateText(StorageFile);
            await JsonSerializer.SerializeAsync(writer.BaseStream, _allowedUsersIds,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save: {}", StorageFile);
        }
    }
}