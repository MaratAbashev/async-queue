using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class TopicConfiguration: IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder
            .HasKey(t => t.Id);
        
        builder
            .HasIndex(t => t.TopicName)
            .IsUnique();
        
        builder
            .Property(t => t.Id)
            .ValueGeneratedOnAdd();
    }
}