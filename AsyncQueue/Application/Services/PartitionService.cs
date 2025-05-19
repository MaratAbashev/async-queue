using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Services;

public class PartitionService(IPartitionRepository partitionRepository,
    ITopicRepository topicRepository) : IPartitionService
{
    public async Task<Partition> AddPartition(string topicName, CancellationToken ct = default)
    {
        return await partitionRepository.AddPartitionAsync(topicName, ct);
    }
    
    public async Task DeletePartition(int partitionId)
    {
        await partitionRepository.DeleteAsync(partition => partition.Id == partitionId);
    }
}