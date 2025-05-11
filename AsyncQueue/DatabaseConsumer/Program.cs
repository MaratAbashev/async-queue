using ConsumerClient;
using ConsumerClient.Abstractions;
using DatabaseConsumer;
using DatabaseConsumer.Abstractions;
using DatabaseConsumer.Services.Database;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHostedService<Worker>();

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
builder.Services.AddDbContext<ConsumerDbContext>(contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
builder.Services.AddSingleton<IDbConsumerRepository, DbConsumerRepository>();
var host = builder.Build();

using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ConsumerDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
try
{
    context.Database.Migrate();
}
catch (Exception ex)
{
    logger.LogError(ex, ex.Message);
}
host.Run();