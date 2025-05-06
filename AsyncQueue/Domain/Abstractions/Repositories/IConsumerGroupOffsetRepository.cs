using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IConsumerGroupOffsetRepository: IRepository<ConsumerGroupOffset,int>
{
    public Task<Dictionary<int, int>> GetByConsumerGroupIdAsync(int consumerGroupId);
}