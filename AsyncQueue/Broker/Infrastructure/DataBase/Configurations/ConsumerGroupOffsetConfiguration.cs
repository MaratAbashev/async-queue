using Broker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class ConsumerGroupOffsetConfiguration: IEntityTypeConfiguration<ConsumerGroupOffset>
{
    public void Configure(EntityTypeBuilder<ConsumerGroupOffset> builder)
    {
        builder
            .HasKey(cgo => cgo.ConsumerGroupOffsetId);
        
        builder
            .Property(cgo => cgo.ConsumerGroupOffsetId)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(cgo => cgo.Partition)
            .WithMany(p => p.ConsumerGroupOffsets)
            .HasForeignKey(cgo => cgo.PartitionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(cgo => cgo.ConsumerGroup)
            .WithMany(cg => cg.ConsumerGroupOffsets)
            .HasForeignKey(cgo => cgo.ConsumerGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .Property(cgo => cgo.Offset)
            .HasDefaultValue(0);
    }
}