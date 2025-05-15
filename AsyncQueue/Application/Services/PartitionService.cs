using Domain.Abstractions.Repositories;
using Domain.Entities;

namespace Application.Services;

public class PartitionService(IPartitionRepository partitionRepository,
    ITopicRepository topicRepository)
{
    public async Task<Partition> AddPartition(string topicName, CancellationToken ct = default)
    {
        return await partitionRepository.AddPartitionAsync(topicName, ct);
    }
}