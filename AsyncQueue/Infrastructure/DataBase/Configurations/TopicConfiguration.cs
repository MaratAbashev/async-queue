using Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class TopicConfiguration: IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder
            .HasKey(t => t.TopicId);
        
        builder
            .Property(t => t.TopicId)
            .ValueGeneratedOnAdd();
    }
}