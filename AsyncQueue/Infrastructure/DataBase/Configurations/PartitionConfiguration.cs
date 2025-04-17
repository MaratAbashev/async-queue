using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class PartitionConfiguration: IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder
            .HasKey(p => p.Id);
        
        builder
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
        
        builder
            .HasOne(p => p.Topic)
            .WithMany(t => t.Partitions)
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
    }
}