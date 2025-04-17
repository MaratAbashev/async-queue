using Domain.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TopicRepository(BrokerDbContext context): 
    Repository<Topic,int>(context),
    ITopicRepository
{
    public async Task<Dictionary<int, IEnumerable<Message>>> GroupMessagesInTopicByPartitionAsync(string topic)
    {
        return await _dbSet
            .Include(t => t.Partitions)
            .ThenInclude(p => p.Messages)
            .SelectMany(t => t.Partitions)
            .GroupBy(p => p.Id)
            .ToDictionaryAsync(
                t => t.Key, 
                t => t
                    .SelectMany(p => p.Messages));
    }
}