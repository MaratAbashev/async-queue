using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class ConsumerGroupConfiguration: IEntityTypeConfiguration<ConsumerGroup>
{
    public void Configure(EntityTypeBuilder<ConsumerGroup> builder)
    {
        builder
            .HasKey(cg => cg.ConsumerGroupId);

        builder
            .Property(cg => cg.ConsumerGroupId)
            .ValueGeneratedOnAdd();

        builder
            .HasOne(cg => cg.Topic)
            .WithMany(t => t.ConsumerGroups)
            .HasForeignKey(cg => cg.TopicId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(cg => cg.Consumers)
            .WithOne(c => c.ConsumerGroup)
            .HasForeignKey(c => c.ConsumerGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}