using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class ConsumerConfiguration: IEntityTypeConfiguration<Consumer>
{
    public void Configure(EntityTypeBuilder<Consumer> builder)
    {
        builder
            .HasKey(c => c.ConsumerId);

        builder
            .HasOne(c => c.ConsumerGroup)
            .WithMany(cg => cg.Consumers)
            .HasForeignKey(c => c.ConsumerGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}