using Domain.Entities;
using Domain.Models;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace BackgroundBrokerServices.BackgroundJobs;

public class UnprocessedMessagesHandleJob(
    ILogger<UnprocessedMessagesHandleJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration config)
{
    public async Task ExecuteUnprocessedMessagesHandle()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<BrokerDbContext>();
        var trashTopicName = config["MessageBroker:TrashTopic"];
        
        var trashPartitionId = context.Topics
            .Include(t => t.Partitions)
            .FirstOrDefault(t => t.TopicName == trashTopicName)?
            .Partitions.FirstOrDefault()?.Id;

        if (trashPartitionId == null)
        {
            logger.LogInformation("Trash partition id not found");
            return;
        }
        
        var infinitelyProcessingMessages = context.ConsumerGroupMessageStatuses
            .Include(cgms => cgms.Consumer)
            .Include(cgms => cgms.Message)
            .Where(cgms => cgms.Status == MessageStatus.Processing && cgms.Consumer.IsDeleted)
            .Select(cgms => cgms.Message)
            .ToHashSet();

        foreach (var message in infinitelyProcessingMessages)
        {
            message.PartitionId = trashPartitionId.Value;
            logger.LogInformation($"Infinity processing message {message.Id} moved to trash topic.");
        }
        
        //сделать с pending сообщениями
        var unprocessedPendingMessages = context.Messages
            .Include(m => m.ConsumerGroupMessageStatuses)
            .GroupBy(m => m.PartitionId)
            .ToDictionary(group => group.Key, group => group
                .OrderByDescending(m => m.PartitionNumber)
                .SkipWhile(m => m.ConsumerGroupMessageStatuses.Any(cgms => cgms.Status == MessageStatus.Pending))
                .Where(m => m.ConsumerGroupMessageStatuses.Any(cgms => cgms.Status == MessageStatus.Pending))
                .ToList())
            .SelectMany(pair => pair.Value);
        
        foreach (var message in unprocessedPendingMessages)
        {
            message.PartitionId = trashPartitionId.Value;
            logger.LogInformation($"Unprocessed message {message.Id} moved to trash topic.");
        }
        await context.SaveChangesAsync();
    }
}