using Domain.Entities;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class DbInitializerService(
    IServiceProvider serviceProvider,
    ILogger<DbInitializerService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrokerDbContext>();
        var brokerStartingData = scope.ServiceProvider.GetRequiredService<IOptions<BrokerStartingData>>().Value;

        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Seeding initial data...");
            await SeedDataAsync(dbContext, brokerStartingData, cancellationToken);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while initializing database");
            throw;
        }
    }

    private async Task SeedDataAsync(
    BrokerDbContext context,
    BrokerStartingData brokerStartingData,
    CancellationToken cancellationToken)
{
    if (!await context.Topics.AnyAsync(cancellationToken))
    {
        var topics = brokerStartingData.Topics
            .Select(t => 
                new Topic
                {
                    TopicName = t.TopicName
                })
            .ToList();
        
        await context.Topics.AddRangeAsync(topics, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var partitions = new List<Partition>();
        foreach (var topic in topics)
        {
            var topicConfig = brokerStartingData.Topics.First(t => t.TopicName == topic.TopicName);
            
            for (var i = 0; i < topicConfig.PartitionCount; i++)
            {
                partitions.Add(new Partition
                {
                    TopicId = topic.Id
                });
            }
        }
        
        await context.Partitions.AddRangeAsync(partitions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        var consumerGroups = new List<ConsumerGroup>();
        foreach (var topic in topics)
        {
            var topicConfig = brokerStartingData.Topics.First(t => t.TopicName == topic.TopicName);

            consumerGroups.AddRange(topicConfig.ConsumerGroups
                .Select(cgName => 
                    new ConsumerGroup
                    {
                        TopicId = topic.Id, 
                        ConsumerGroupName = cgName
                    }));
        }
        
        await context.ConsumerGroups.AddRangeAsync(consumerGroups, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var offsets = new List<ConsumerGroupOffset>();
        foreach (var consumerGroup in consumerGroups)
        {
            var topicPartitions = partitions
                .Where(p => p.TopicId == consumerGroup.TopicId)
                .ToList();

            offsets.AddRange(topicPartitions
                .Select(partition => 
                    new ConsumerGroupOffset
                    {
                        ConsumerGroupId = consumerGroup.Id, 
                        PartitionId = partition.Id, 
                        Offset = 0
                    }));
        }
        
        await context.ConsumerGroupOffsets.AddRangeAsync(offsets, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}