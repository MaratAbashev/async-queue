using Domain.Entities;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace BrokerApi.Filters;

public class DataBaseStartUpFilter(IConfiguration configuration): IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BrokerDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                context.Database.Migrate();
                context.Topics.Add(new Topic
                {
                    TopicName = configuration["MessageBroker:Topic"],
                });
                context.SaveChanges();
                var topicId = context.Topics.First().Id;
                int.TryParse(configuration["MessageBroker:PartitionCount"], out int partitionCount);
                for (int i = 0; i < partitionCount; i++)
                {
                    context.Partitions.Add(new Partition
                    {
                        TopicId = topicId,
                    });
                }

                context.ConsumerGroups.Add(new ConsumerGroup
                {
                    TopicId = topicId,
                    ConsumerGroupName = configuration["MessageBroker:ConsumerGroup"],
                });
                context.SaveChanges();
                var partitionIds = context.Partitions.Select(p => p.Id).ToList();
                var consumerGroupId = context.ConsumerGroups.First().Id;
                foreach (var partitionId in partitionIds)
                {
                    context.ConsumerGroupOffsets.Add(new ConsumerGroupOffset
                    {
                        PartitionId = partitionId,
                        Offset = 0,
                        ConsumerGroupId = consumerGroupId
                    });
                }

                context.SaveChanges();
            }
            catch(Exception ex)
            {
                logger.LogCritical(ex.Message);
            }
        };
    }
}