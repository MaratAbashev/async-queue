using Domain.Abstractions.Services;
using Domain.Entities;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

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
        var consumersUrls = await context.Consumers
            .Select(c => c.Address)
            .Distinct()
            .ToListAsync();
        
        foreach (var consumerUrl in consumersUrls)
        {
            try
            {
                var isHealthy = await healthCheckService.CheckConsumerHealthAsync(consumerUrl);
                
                var currentConsumers = context.Consumers
                    .Include(c => c.Partitions)!
                    .ThenInclude(p => p.Consumers)
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
            //consumer.Partitions?.Clear();
            foreach (var partition in consumer.Partitions)
            {
                if (partition.Consumers?.Contains(consumer) ?? false)
                {
                    partition.Consumers.Remove(consumer);
                }

                if (partition.Consumers?.Count == 0)
                {
                    var newConsumer = context.Consumers
                        .Where(c => !c.IsDeleted && c.ConsumerGroupId == consumer.ConsumerGroupId)
                        .Include(consumer => consumer.Partitions)
                        .AsEnumerable()
                        .MinBy(c =>
                        {
                            if (c.Partitions != null) return c.Partitions.Count;
                            return 0;
                        });
                    if (newConsumer != null)
                    {
                        if (newConsumer.Partitions != null) newConsumer.Partitions.Add(partition);
                    }
                }
            }
        }
        await context.SaveChangesAsync();
    }
}