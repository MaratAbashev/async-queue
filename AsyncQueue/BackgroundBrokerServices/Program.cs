using BackgroundBrokerServices.BackgroundJobs;
using Domain.Abstractions.Services;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Infrastructure.Consumers.Services;
using Infrastructure.DataBase;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "broker-api")
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ELASTICSEARCH_HOSTS"]!))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "broker-api-logs-{0:yyyy.MM.dd}",
    })
    .CreateLogger();

Log.Logger = logger;


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

builder.Logging
    .ClearProviders()
    .AddFilter("Microsoft", LogLevel.Error)
    .AddFilter("System", LogLevel.Error)
    .AddFilter("Microsoft.AspNetCore", LogLevel.Error);
builder.Host.UseSerilog(logger, dispose: true);

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

app.UseSerilogRequestLogging(opt =>
{
    opt.GetLevel = (httpContext, _, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        return LogEventLevel.Verbose;
    };
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