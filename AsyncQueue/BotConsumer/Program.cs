using BotConsumer;
using BotConsumer.Abstractions;
using BotConsumer.Services;
using ConsumerClient.Abstractions;
using ConsumerClient;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHostedService<Worker>();
int batchSize = 5;
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
    var consumerGroup = builder.Configuration["Bot:ConsumerGroup"];
    if (string.IsNullOrEmpty(brokerUrl))
    {
        throw new NullReferenceException("Please provide a broker url");
    }

    if (string.IsNullOrEmpty(consumerGroup))
    {
        throw new NullReferenceException("Please provide a consumer group");
    }
    return new ConsumerClient<string>(consumerGroup, brokerUrl, batchSize, builder.Configuration["BotConsumer:Host"]);
});
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

var app = builder.Build();

app.Run();