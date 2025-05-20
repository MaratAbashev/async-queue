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
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString(nameof(BrokerDbContext)))
            .UseLoggerFactory(CreateLoggerFactory())
            .EnableSensitiveDataLogging();
    }
    
    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Warning)
            .AddConsole());
    }
}