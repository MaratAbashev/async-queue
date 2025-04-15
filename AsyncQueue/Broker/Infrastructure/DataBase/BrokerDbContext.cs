using Broker.Infrastructure.DataBase.Configurations;
using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Broker.Infrastructure.DataBase;

public class BrokerDbContext(DbContextOptions<BrokerDbContext> options): DbContext(options)
{
    public DbSet<Consumer> Consumers { get; set; }
    public DbSet<ConsumerGroup> ConsumerGroups { get; set; }
    public DbSet<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public DbSet<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Partition> Partitions { get; set; }
    public DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupMessageStatusConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupOffsetConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new PartitionConfiguration());
        modelBuilder.ApplyConfiguration(new TopicConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}