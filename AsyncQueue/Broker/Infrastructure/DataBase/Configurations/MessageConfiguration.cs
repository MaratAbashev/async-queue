using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class MessageConfiguration: IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder
            .HasKey(m => m.MessageId);
        
        builder
            .HasOne(m => m.Partition)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.PartitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}