using Telegram.Bot;
using TelegramConsumer;
using TelegramConsumer.Abstractions;
using TelegramConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
{
    var token = builder.Configuration["TelegramBotClientOptions:Token"];
    if (string.IsNullOrEmpty(token))
    {
        throw new NullReferenceException("Please provide a valid token");
    }
    return new TelegramBotClient(token);
});
builder.Services.AddHttpClient<IConsumerService, ConsumerService>(client =>
{
    var brokerUrl = builder.Configuration["MessageBroker"];
    if (string.IsNullOrEmpty(brokerUrl))
    {
        throw new NullReferenceException("Please provide a broker url");
    }
    client.BaseAddress = new Uri(brokerUrl);
});
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();


var host = builder.Build();
host.Run();