using System.Linq.Expressions;
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
            .Include(t => t.Partitions)
            .ThenInclude(p => p.Consumers)
            .FirstOrDefaultAsync(t => t.TopicName == topicName, cancellationToken);
        if (topic == null)
            throw new InvalidOperationException($"Topic with name {topicName} not found");
        var partition = new Partition
        {
            TopicId = topic.Id,
        };
        await context.Partitions.AddAsync(partition, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var consumerGroups = await context.ConsumerGroups
            .Where(cg => cg.TopicId == topic.Id)
            .Include(cg => cg.Consumers)
            .ThenInclude(c => c.Partitions)
            .ToListAsync(cancellationToken);

        foreach (var cg in consumerGroups)
        {
            await context.ConsumerGroupOffsets.AddAsync(new ConsumerGroupOffset
            {
                Offset = 0,
                PartitionId = partition.Id,
                ConsumerGroupId = cg.Id
            }, cancellationToken);

            var consumer = cg.Consumers
                .Select(c => 
                {
                    c.Partitions ??= [];
                    return c;
                })
                .OrderBy(c => c.Partitions!.Count)
                .FirstOrDefault();

            consumer?.Partitions!.Add(partition);
        }
        await context.SaveChangesAsync(cancellationToken);
        return partition;
    }

    public override async Task DeleteAsync(Expression<Func<Partition, bool>> predicate)
    {
        await base.DeleteAsync(predicate);
        var partitionsToDelete = await context.Partitions
            .Where(predicate)
            .Include(p => p.Consumers)
            .ToListAsync();

        if (partitionsToDelete.Count != 0)
        {
            var allConsumers = partitionsToDelete
                .SelectMany(p => p.Consumers)
                .Distinct()
                .ToList();

            foreach (var consumer in allConsumers)
            {
                consumer.Partitions = consumer.Partitions?
                    .Where(p => !p.IsDeleted)
                    .ToList();
            }

            await context.SaveChangesAsync();
        }
    }
}