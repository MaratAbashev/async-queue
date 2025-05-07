using ConsumerClient;
using ConsumerClient.Abstractions;
using Telegram.Bot;
using TelegramConsumer;
using TelegramConsumer.Abstractions;
using TelegramConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();
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
builder.Services.AddSingleton<IConsumerClient<string>, ConsumerClient<string>>(_ =>
{
    var brokerUrl = builder.Configuration["MessageBroker:Host"];
    var consumerGroup = builder.Configuration["MessageBroker:ConsumerGroup"];
    if (string.IsNullOrEmpty(brokerUrl))
    {
        throw new NullReferenceException("Please provide a broker url");
    }

    if (string.IsNullOrEmpty(consumerGroup))
    {
        throw new NullReferenceException("Please provide a consumer group");
    }
    return new ConsumerClient<string>(consumerGroup, brokerUrl);
});
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();


var host = builder.Build();
host.Run();