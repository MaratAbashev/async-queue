using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerGroupMessageStatusRepository(BrokerDbContext context) :
    Repository<ConsumerGroupMessageStatus, long>(context),
    IConsumerGroupMessageStatusRepository
{
    public async Task<List<ConsumerGroupMessageStatus>> GetAllByStatusAndConsumerGroupIdAsync(int consumerGroupId,
        MessageStatus status)
    {
        var result = await _dbSet
            .Where(cgms => cgms.ConsumerGroupId == consumerGroupId
            && cgms.Status == status)
            .Include(cgms => cgms.Message)
            .ToListAsync();
        return result;
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