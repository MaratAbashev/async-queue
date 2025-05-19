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
            .Select(cgms => cgms.Message);

        foreach (var message in infinitelyProcessingMessages)
        {
            message.PartitionId = trashPartitionId.Value;
            logger.LogInformation($"Infinity processing message {message.Id} moved to trash topic.");
        }
        
        //сделать с pending сообщениями
        var unprocessedPendingMessages = context.ConsumerGroupMessageStatuses;
        await context.SaveChangesAsync();
    }
}