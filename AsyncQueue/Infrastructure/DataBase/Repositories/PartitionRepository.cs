using Domain.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class PartitionRepository(BrokerDbContext context): Repository<Partition, int>(context),
    IPartitionRepository
{
    public async Task<Partition> AddPartitionAsync(string topicName, CancellationToken cancellationToken = default)
    {
        var topic = await context.Topics
            .FirstOrDefaultAsync(t => t.TopicName == topicName, cancellationToken);
        if (topic == null)
            throw new InvalidOperationException($"Topic with name {topicName} not found");
        context.Topics
            .Where(t => t.rrrrrrrrrrrr)
    }
}