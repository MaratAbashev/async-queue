﻿// <auto-generated />
using System;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
    [DbContext(typeof(BrokerDbContext))]
    [Migration("20250513085114_TimeStampMessage")]
    partial class TimeStampMessage
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConsumerPartition", b =>
                {
                    b.Property<Guid>("ConsumersId")
                        .HasColumnType("uuid");

                    b.Property<int>("PartitionsId")
                        .HasColumnType("integer");

                    b.HasKey("ConsumersId", "PartitionsId");

                    b.HasIndex("PartitionsId");

                    b.ToTable("ConsumerPartition");
                });

            modelBuilder.Entity("Domain.Entities.Consumer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ConsumerGroupId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerGroupId");

                    b.ToTable("Consumers");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ConsumerGroupName")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("TopicId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TopicId");

                    b.ToTable("ConsumerGroups");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroupMessageStatus", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("ConsumerGroupId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerGroupId");

                    b.HasIndex("MessageId");

                    b.ToTable("ConsumerGroupMessageStatuses");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroupOffset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ConsumerGroupId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("Offset")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<int>("PartitionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerGroupId");

                    b.HasIndex("PartitionId");

                    b.ToTable("ConsumerGroupOffsets");
                });

            modelBuilder.Entity("Domain.Entities.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<int>("PartitionId")
                        .HasColumnType("integer");

                    b.Property<int>("PartitionNumber")
                        .HasColumnType("integer");

                    b.Property<string>("ValueJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PartitionId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Domain.Entities.Partition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("TopicId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TopicId");

                    b.ToTable("Partitions");
                });

            modelBuilder.Entity("Domain.Entities.Producer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CurrentSequenceNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Producers");
                });

            modelBuilder.Entity("Domain.Entities.Topic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("TopicName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TopicName")
                        .IsUnique();

                    b.ToTable("Topics");
                });

            modelBuilder.Entity("ConsumerPartition", b =>
                {
                    b.HasOne("Domain.Entities.Consumer", null)
                        .WithMany()
                        .HasForeignKey("ConsumersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Partition", null)
                        .WithMany()
                        .HasForeignKey("PartitionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Consumer", b =>
                {
                    b.HasOne("Domain.Entities.ConsumerGroup", "ConsumerGroup")
                        .WithMany("Consumers")
                        .HasForeignKey("ConsumerGroupId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("ConsumerGroup");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroup", b =>
                {
                    b.HasOne("Domain.Entities.Topic", "Topic")
                        .WithMany("ConsumerGroups")
                        .HasForeignKey("TopicId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Topic");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroupMessageStatus", b =>
                {
                    b.HasOne("Domain.Entities.ConsumerGroup", "ConsumerGroup")
                        .WithMany("ConsumerGroupMessageStatuses")
                        .HasForeignKey("ConsumerGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Message", "Message")
                        .WithMany("ConsumerGroupMessageStatuses")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConsumerGroup");

                    b.Navigation("Message");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroupOffset", b =>
                {
                    b.HasOne("Domain.Entities.ConsumerGroup", "ConsumerGroup")
                        .WithMany("ConsumerGroupOffsets")
                        .HasForeignKey("ConsumerGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Partition", "Partition")
                        .WithMany("ConsumerGroupOffsets")
                        .HasForeignKey("PartitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConsumerGroup");

                    b.Navigation("Partition");
                });

            modelBuilder.Entity("Domain.Entities.Message", b =>
                {
                    b.HasOne("Domain.Entities.Partition", "Partition")
                        .WithMany("Messages")
                        .HasForeignKey("PartitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Partition");
                });

            modelBuilder.Entity("Domain.Entities.Partition", b =>
                {
                    b.HasOne("Domain.Entities.Topic", "Topic")
                        .WithMany("Partitions")
                        .HasForeignKey("TopicId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Topic");
                });

            modelBuilder.Entity("Domain.Entities.ConsumerGroup", b =>
                {
                    b.Navigation("ConsumerGroupMessageStatuses");

                    b.Navigation("ConsumerGroupOffsets");

                    b.Navigation("Consumers");
                });

            modelBuilder.Entity("Domain.Entities.Message", b =>
                {
                    b.Navigation("ConsumerGroupMessageStatuses");
                });

            modelBuilder.Entity("Domain.Entities.Partition", b =>
                {
                    b.Navigation("ConsumerGroupOffsets");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Domain.Entities.Topic", b =>
                {
                    b.Navigation("ConsumerGroups");

                    b.Navigation("Partitions");
                });
#pragma warning restore 612, 618
        }
    }
}
