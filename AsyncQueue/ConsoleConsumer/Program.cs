using ConsoleConsumer;
using ConsumerClient.Abstractions;
using ConsumerClient;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHostedService<Worker>();
int batchSize = 5;
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
    return new ConsumerClient<string>(consumerGroup, brokerUrl, batchSize);
});

var host = builder.Build();
host.Run();