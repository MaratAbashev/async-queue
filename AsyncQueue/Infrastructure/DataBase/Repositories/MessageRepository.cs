using Domain.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class MessageRepository(BrokerDbContext context) : 
    Repository<Message, Guid>(context),
    IMessageRepository
{
    public async Task<Dictionary<int,IEnumerable<Message>>> GroupMessagesInTopicByPartitionAsync(string topic)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Partition)
            .ThenInclude(pk => pk.Topic)
            .Where(m => m.Partition.Topic.TopicName == topic)
            .GroupBy(m => m.PartitionId)
            .ToDictionaryAsync(m => m.Key, m => m.AsEnumerable());
    }
}