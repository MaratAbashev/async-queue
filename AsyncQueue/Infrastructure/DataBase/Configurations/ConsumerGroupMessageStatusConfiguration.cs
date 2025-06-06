using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class ConsumerGroupMessageStatusConfiguration: IEntityTypeConfiguration<ConsumerGroupMessageStatus>
{
    public void Configure(EntityTypeBuilder<ConsumerGroupMessageStatus> builder)
    {
        builder
            .HasKey(cgms => cgms.Id);
        
        builder
            .Property(cgms => cgms.Id)
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
        
        builder
            .HasOne(cgms => cgms.Consumer)
            .WithMany(cgms => cgms.ConsumerGroupMessageStatuses)
            .HasForeignKey(cgms => cgms.ConsumerId)
            .OnDelete(DeleteBehavior.SetNull);
            
    }
}