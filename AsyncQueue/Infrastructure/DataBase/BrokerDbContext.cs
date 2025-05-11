using Domain.Entities;
using Infrastructure.DataBase.Configurations;
using Infrastructure.DataBase.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.DataBase;

public class BrokerDbContext(
    DbContextOptions<BrokerDbContext> options, 
    IConfiguration configuration, IOptions<BrokerStartingData> brokerStartingDataOptions): DbContext(options)
{
    public DbSet<Consumer> Consumers { get; set; }
    public DbSet<ConsumerGroup> ConsumerGroups { get; set; }
    public DbSet<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public DbSet<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Partition> Partitions { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Producer> Producers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupMessageStatusConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerGroupOffsetConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new PartitionConfiguration());
        modelBuilder.ApplyConfiguration(new TopicConfiguration());
        modelBuilder.ApplyConfiguration(new ProducerConfiguration());
        AddStartingData(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString(nameof(BrokerDbContext)))
            .UseLoggerFactory(CreateLoggerFactory())
            .EnableSensitiveDataLogging();
    }

    private void AddStartingData(ModelBuilder modelBuilder)
    {
        var brokerStartingData = brokerStartingDataOptions.Value;
        
        int topicId = 0;
        int partitionId = 0;
        int consumerGroupId = 0;
        int consumerGroupOffsetId = 0;

        var topicEntities = new List<Topic>();
        var partitionEntities = new List<Partition>();
        var consumerGroupEntities = new List<ConsumerGroup>();
        var consumerGroupOffsetEntities = new List<ConsumerGroupOffset>();

        foreach (var topicConfig in brokerStartingData.Topics)
        {
            var currentTopicId = ++topicId;
            topicEntities.Add(new Topic
            {
                Id = currentTopicId,
                TopicName = topicConfig.TopicName
            });
            var topicPartitions = new List<Partition>();
            for (int i = 0; i < topicConfig.PartitionCount; i++)
            {
                var partition = new Partition
                {
                    Id = ++partitionId,
                    TopicId = currentTopicId
                };
                topicPartitions.Add(partition);
                partitionEntities.Add(partition);
            }
            foreach (var consumerGroupName in topicConfig.ConsumerGroups)
            {
                var currentConsumerGroupId = ++consumerGroupId;
                consumerGroupEntities.Add(new ConsumerGroup
                {
                    Id = currentConsumerGroupId,
                    TopicId = currentTopicId,
                    ConsumerGroupName = consumerGroupName
                });
                consumerGroupOffsetEntities.AddRange(
                    topicPartitions.Select(partition => 
                        new ConsumerGroupOffset
                        {
                            Id = ++consumerGroupOffsetId, ConsumerGroupId = currentConsumerGroupId, PartitionId = partition.Id
                        }));
            }
        }

        modelBuilder.Entity<Topic>().HasData(topicEntities);
        modelBuilder.Entity<Partition>().HasData(partitionEntities);
        modelBuilder.Entity<ConsumerGroup>().HasData(consumerGroupEntities);
        modelBuilder.Entity<ConsumerGroupOffset>().HasData(consumerGroupOffsetEntities);
    }
    
    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }
}