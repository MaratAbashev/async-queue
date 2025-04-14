using DatabaseConsumer;
using DatabaseConsumer.Abstractions;
using DatabaseConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient<IConsumerService<string>, ConsumerService<string>>(client =>
{
    var brokerUrl = builder.Configuration["MessageBroker"];
    if (string.IsNullOrEmpty(brokerUrl))
    {
        throw new NullReferenceException("Please provide a broker url");
    }
    client.BaseAddress = new Uri(brokerUrl);
});
builder.Services.AddSingleton<IConsumerService<string>, ConsumerService<string>>();
var host = builder.Build();
host.Run();
