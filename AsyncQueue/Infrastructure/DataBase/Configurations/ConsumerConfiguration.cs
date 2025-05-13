using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class ConsumerConfiguration: IEntityTypeConfiguration<Consumer>
{
    public void Configure(EntityTypeBuilder<Consumer> builder)
    {
        builder
            .HasKey(c => c.Id);

        builder
            .HasOne(c => c.ConsumerGroup)
            .WithMany(cg => cg.Consumers)
            .HasForeignKey(c => c.ConsumerGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(c => c.Partitions)
            .WithMany(p => p.Consumers);
    }
}