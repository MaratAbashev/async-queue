using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class ConsumerGroupConfiguration: IEntityTypeConfiguration<ConsumerGroup>
{
    public void Configure(EntityTypeBuilder<ConsumerGroup> builder)
    {
        builder
            .HasKey(cg => cg.Id);

        builder
            .Property(cg => cg.Id)
            .ValueGeneratedOnAdd();

        builder
            .HasOne(cg => cg.Topic)
            .WithMany(t => t.ConsumerGroups)
            .HasForeignKey(cg => cg.TopicId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(cg => cg.Consumers)
            .WithOne(c => c.ConsumerGroup)
            .HasForeignKey(c => c.ConsumerGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}