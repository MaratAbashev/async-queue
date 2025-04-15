﻿using DatabaseConsumer.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseConsumer.Services.Database.Configurations;

public class MessageConfiguration: IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder
            .HasKey(m => m.MessageId);
        builder
            .Property(m => m.MessageId)
            .ValueGeneratedOnAdd();
    }
}