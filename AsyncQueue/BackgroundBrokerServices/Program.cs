using BackgroundBrokerServices.BackgroundJobs;
using Domain.Abstractions.Services;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Infrastructure.Consumers.Services;
using Infrastructure.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<BrokerDbContext>();

builder.Services.AddSingleton<ConsumerHealthCheckJob>();
builder.Services.AddSingleton<UnprocessedMessagesHandleJob>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();

builder.Services.AddHttpClient("consumer-bot", client =>
{
    client.BaseAddress = new Uri("http://consumer-bot:8080/");
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddHttpClient("consumer-console", client =>
{
    client.BaseAddress = new Uri("http://consumer-console:8080/");
    client.Timeout = TimeSpan.FromSeconds(3);
});
builder.Services.AddHttpClient("consumer-db", client =>
{
    client.BaseAddress = new Uri("http://consumer-db:8080/");
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddHangfire(configuration => 
    configuration.UsePostgreSqlStorage(options => 
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Hangfire"))));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

RecurringJob.AddOrUpdate("consumer-health-check",
    () => app.Services.GetService<ConsumerHealthCheckJob>()!.ExecuteHealthCheck(),
    Cron.Minutely);

RecurringJob.AddOrUpdate("unprocessed-messages-handle",
    () => app.Services.GetService<UnprocessedMessagesHandleJob>()!.ExecuteUnprocessedMessagesHandle(),
    Cron.Minutely);

app.Run();

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}