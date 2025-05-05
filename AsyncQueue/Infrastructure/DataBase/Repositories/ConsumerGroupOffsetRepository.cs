using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Domain.Models.ConsumersDtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerGroupOffsetRepository(BrokerDbContext context) :
    Repository<ConsumerGroupOffset, int>(context),
    IConsumerGroupOffsetRepository
{
    public async Task<Dictionary<int, int>> GetByConsumerGroupIdAsync(int consumerGroupId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.ConsumerGroupId == consumerGroupId)
            .ToDictionaryAsync(o => o.PartitionId, o => o.Offset);
    }
}