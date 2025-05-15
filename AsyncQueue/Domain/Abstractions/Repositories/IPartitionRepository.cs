using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IPartitionRepository: IRepository<Partition, int>
{
    Task<Partition> AddPartitionAsync(string topicName, CancellationToken cancellationToken = default);
}