using Domain.Entities;
using Infrastructure.DataBase;

namespace BrokerApi;

public class DataBaseStartUp(BrokerDbContext context)
{
    public async Task Initialize()
    {
        await context.Topics.AddAsync(new Topic
        {
            Id = 0,
            TopicName = "topic1"
        });
        await context.Partitions.AddAsync(new Partition
        {
            TopicId = 0
        });
        await context.Partitions.AddAsync(new Partition
        {
            TopicId = 0
        });
        await context.ConsumerGroups.AddAsync(new ConsumerGroup
        {
            TopicId = 0,
            ConsumerGroupName = "group1"
        });
    }
}