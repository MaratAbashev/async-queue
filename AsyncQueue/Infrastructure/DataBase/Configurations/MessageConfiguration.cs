using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class MessageConfiguration: IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder
            .HasKey(m => m.Id);
        
        builder
            .HasOne(m => m.Partition)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.PartitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}