using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class PartitionConfiguration: IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder
            .HasKey(p => p.PartitionId);
        
        builder
            .Property(p => p.PartitionId)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(p => p.Topic)
            .WithMany(t => t.Partitions)
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
    }
}