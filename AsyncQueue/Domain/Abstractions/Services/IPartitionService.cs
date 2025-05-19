using Domain.Entities;

namespace Application.Services;

public interface IPartitionService
{
    Task<Partition> AddPartition(string topicName, CancellationToken ct = default);
    Task DeletePartition(int partitionId);
}