using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface IPartitionService
{
    Task<Partition> AddPartition(string topicName, CancellationToken ct = default);
    Task DeletePartition(int partitionId);
}