using DatabaseConsumer.Models.Entities;
using DatabaseConsumer.Services.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConsumer.Services.Database;

public class ConsumerDbContext(DbContextOptions<ConsumerDbContext> options,
    IConfiguration configuration): DbContext(options)
{
    public DbSet<MessageEntity> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("ConsumerDb"))
            .UseLoggerFactory(CreateLoggerFactory())
            .EnableSensitiveDataLogging();
    }

    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }
}