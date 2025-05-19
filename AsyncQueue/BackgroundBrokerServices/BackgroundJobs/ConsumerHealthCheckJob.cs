using Domain.Abstractions.Services;
using Domain.Entities;
using Infrastructure.DataBase;

namespace BackgroundBrokerServices.BackgroundJobs;

public class ConsumerHealthCheckJob(
    ILogger<ConsumerHealthCheckJob> logger,
    IHealthCheckService healthCheckService,
    IServiceScopeFactory serviceScopeFactory)
{
    private BrokerDbContext context;
    public async Task ExecuteHealthCheck()
    {
        using var scope = serviceScopeFactory.CreateScope();
        context = scope.ServiceProvider.GetService<BrokerDbContext>()!;
        var consumersUrls = context.Consumers
            .Select(c => c.Address)
            .Distinct();
        
        foreach (var consumerUrl in consumersUrls)
        {
            try
            {
                var isHealthy = await healthCheckService.CheckConsumerHealthAsync(consumerUrl);
                
                var currentConsumers = context.Consumers
                    .Where(c => c.Address == consumerUrl && !c.IsDeleted);
                
                if (!isHealthy)
                {
                    await UpdateDeadConsumers(currentConsumers);
                    logger.LogInformation($"Consumer {consumerUrl} is unhealthy");
                }
                else
                {
                    var deadConsumers = currentConsumers
                        .OrderByDescending(c => c.RegisteredAt)
                        .Skip(1);
                    await UpdateDeadConsumers(deadConsumers);
                    logger.LogInformation($"Consumer '{consumerUrl}' is healthy");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking health for consumer {consumerUrl}");
            }
        }
    }
    
    private async Task UpdateDeadConsumers(IEnumerable<Consumer> consumers)
    {
        foreach (var consumer in consumers)
        {
            consumer.IsDeleted = true;
            consumer.Partitions?.Clear();
        }
        await context.SaveChangesAsync();
    }
}