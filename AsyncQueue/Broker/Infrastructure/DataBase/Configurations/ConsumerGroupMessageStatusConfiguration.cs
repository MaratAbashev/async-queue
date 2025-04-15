using Broker.Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Infrastructure.DataBase.Configurations;

public class ConsumerGroupMessageStatusConfiguration: IEntityTypeConfiguration<ConsumerGroupMessageStatus>
{
    public void Configure(EntityTypeBuilder<ConsumerGroupMessageStatus> builder)
    {
        builder
            .HasKey(cgms => cgms.ConsumerGroupMessageStatusId);
        
        builder
            .Property(cgms => cgms.ConsumerGroupMessageStatusId)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(cgms => cgms.Message)
            .WithMany(m => m.ConsumerGroupMessageStatuses)
            .HasForeignKey(cgms => cgms.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(cgms => cgms.ConsumerGroup)
            .WithMany(cg => cg.ConsumerGroupMessageStatuses)
            .HasForeignKey(cgms => cgms.ConsumerGroupId)
            .OnDelete(DeleteBehavior.Cascade);
            
    }
}