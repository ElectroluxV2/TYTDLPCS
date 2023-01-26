using System.Net;
using Telegram.Bot;
using TyranoKurwusBot;
using TyranoKurwusBot.Controllers;
using TyranoKurwusBot.Core.Common;
using TyranoKurwusBot.Extensions;
using TyranoKurwusBot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(x => x.Listen(IPAddress.Any, 8443, o =>
{
    // Generate certificate based on domain of HostAddress
    var hostAddress = builder.Configuration.GetValue<string>("TelegramBotConfiguration:HostAddress") ?? throw new Exception("Missing TelegramBotConfiguration:HostAddress");
    var domain = new Uri(hostAddress).Host;
    GenerateSslCertificate.Generate(domain);

    o.UseHttps(GenerateSslCertificate.Certificate);
}));

// Add services to the container.

// The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
// incoming webhook updates and send serialized responses back.
// Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
//   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-6.0#add-newtonsoftjson-based-json-format-support
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup Bot configuration
var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);

var botConfiguration = botConfigurationSection.Get<BotConfiguration>()!;

// Register named HttpClient to get benefits of IHttpClientFactory
// and consume it with ITelegramBotClient typed client.
// More read:
//  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
//  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    var botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken); // For normal client we always use telegram api
                    return new TelegramBotClient(options, httpClient);
                });

builder.Services.AddTransient<TelegramPushBot>(sp =>
                {
                    TelegramBotClientOptions options = new(botConfiguration.BotToken, botConfiguration.BaseTelegramUrl); // For our client we use our api
                    return new TelegramPushBot(options, sp.GetService<ILogger<TelegramPushBot>>()!);
                });

// Business-logic services
builder.Services.AddScoped<UpdateHandlers>();
builder.Services.AddScoped<VideoRequestService>();

builder.Services
    .AddSingleton<AllowedUsers>()
    .AddHostedService(services => services.GetService<AllowedUsers>()!);

// There are several strategies for completing asynchronous tasks during startup.
// Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
builder.Services.AddTransient<ConfigureWebhook>(); // We use this as transient due to fact that this needs to run after kestrel
builder.Services.AddHostedService<UpdateDownloaders>();

var app = builder.Build();

// Telegram verifies certificate during set webhook request, so we need to post it after app is running
app.Lifetime.ApplicationStarted.Register(() => _ = Task.Run(async () => await app.Services.GetService<ConfigureWebhook>()!.StartAsync()));
app.Lifetime.ApplicationStopping.Register(() => _ = Task.Run(async () => await app.Services.GetService<ConfigureWebhook>()!.StopAsync()));

// Construct webhook route from the Route configuration parameter
// It is expected that BotController has single method accepting Update
app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();