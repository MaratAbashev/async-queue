using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerGroupMessageStatusRepository(BrokerDbContext context) :
    Repository<ConsumerGroupMessageStatus, long>(context),
    IConsumerGroupMessageStatusRepository
{
    public async Task<List<ConsumerGroupMessageStatus>> GetAllPendingByConsumerGroupIdAsync(int consumerGroupId)
    {
        return await _dbSet
            .Where(cgms => cgms.ConsumerGroupId == consumerGroupId
            && cgms.Status == MessageStatus.Pending)
            .Include(cgms => cgms.Message)
            .ToListAsync();
    }

    public async Task UpdateConsumerGroupMessageStatusAsync(IEnumerable<ConsumerGroupMessageStatus> consumerGroupMessageStatuses,
        MessageStatus messageStatus)
    {
        foreach (var consumerGroupMessageStatus in consumerGroupMessageStatuses)
        {
            consumerGroupMessageStatus.Status = messageStatus;
        }
        await context.SaveChangesAsync();
    }
}