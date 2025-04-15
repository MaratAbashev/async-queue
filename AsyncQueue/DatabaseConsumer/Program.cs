using DatabaseConsumer;
using DatabaseConsumer.Abstractions;
using DatabaseConsumer.Services;
using DatabaseConsumer.Services.Database;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddDbContext<ConsumerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConsumerDb"));
}, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
builder.Services.AddSingleton<IConsumerService<string>, ConsumerService<string>>();
builder.Services.AddSingleton<IDbConsumerRepository, DbConsumerRepository>();
var host = builder.Build();
host.Run();