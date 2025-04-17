using Domain.Entities;
using Infrastructure.DataBase;

namespace BrokerApi;

public class DataBaseStartUp(BrokerDbContext context)
{
    public async Task Initialize()
    {
        await context.Topics.AddAsync(new Topic
        {
            TopicName = "topic1"
        });
        await context.SaveChangesAsync();
        var topicId = context.Topics.First().Id;
        await context.Partitions.AddAsync(new Partition
        {
            TopicId = topicId,
        });
        await context.Partitions.AddAsync(new Partition
        {
            TopicId = topicId,
        });
        await context.ConsumerGroups.AddAsync(new ConsumerGroup
        {
            TopicId = topicId,
            ConsumerGroupName = "group1"
        });
        await context.SaveChangesAsync();
    }
}