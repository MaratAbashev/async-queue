﻿using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using Domain.Models.ConsumersDtos;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ConsumerService(BrokerDbContext context, IConsumerRepository consumerRepository,
    IConsumerGroupRepository consumerGroupRepository,
    IConsumerGroupOffsetRepository consumerGroupOffsetRepository,
    IConsumerGroupMessageStatusRepository consumerGroupMessageStatusRepository,
    ILogger<ConsumerService> logger) : IConsumerService
{
    public async Task<ConsumerRegisterResponse> RegisterConsumerAsync(ConsumerRegisterRequest consumerRegisterRequest)
    {
        var consumerGroup = await consumerGroupRepository.GetByFilterAsync(cg =>
            cg.ConsumerGroupName == consumerRegisterRequest.ConsumerGroup);
        if (consumerGroup == null)
        {
            logger.LogInformation($"ConsumerGroup {consumerRegisterRequest.ConsumerGroup} not found");
            return new ConsumerRegisterResponse
            {
                ConsumerId = Guid.Empty,
                ProcessingStatus = ProcessingStatus.Wrong,
                Message = $"Consumer Group with name {consumerRegisterRequest.ConsumerGroup} does not exist"
            };
        }
        var consumer = new Consumer
        {
            Id = Guid.NewGuid(),
            ConsumerGroupId = consumerGroup.Id,
            Address = consumerRegisterRequest.Address,
            RegisteredAt = DateTime.UtcNow,
        };
        
        try
        {
            await consumerRepository.AddAsync(consumer);
            
            var consumerTopicId = context.ConsumerGroups
                .Include(cg => cg.Topic)
                .First(cg => cg.Id == consumer.ConsumerGroupId)
                .TopicId;
            
            var partitionsWithConsumers = context.Partitions
                .Where(p => p.TopicId == consumerTopicId)
                .Include(p => p.Consumers)
                .AsEnumerable();
            
            var freePartitions = partitionsWithConsumers
                .Where(p =>
                    p.Consumers != null && (p.Consumers.All(c => c.ConsumerGroupId != consumerGroup.Id) || p.Consumers.Count == 0));
            
            if (freePartitions.Count() != 0)
            {
                foreach (var partition in freePartitions)
                {
                    partition.Consumers.Add(consumer);
                }
                await context.SaveChangesAsync();
                return new ConsumerRegisterResponse
                {
                    ConsumerId = consumer.Id,
                    ProcessingStatus = ProcessingStatus.Success
                };
            }
            
            var freePartition = partitionsWithConsumers
                .Where(p => p.Consumers != null && p.Consumers.Any(c => c.ConsumerGroupId == consumerGroup.Id))
                .ToLookup(p => p.Consumers.Count)
                .FirstOrDefault(group => group.Count() > 1)
                .FirstOrDefault();

            if (freePartition == null)
            {
                logger.LogInformation($"No free partition found for consumer {consumer.Id} in group {consumerGroup.ConsumerGroupName}");
                return new ConsumerRegisterResponse
                {
                    ConsumerId = consumer.Id,
                    ProcessingStatus = ProcessingStatus.Success,
                    Message = "No free partition found"
                };
            }
            
            var previousConsumer = freePartition.Consumers!
                .FirstOrDefault(c => c.ConsumerGroupId == consumerGroup.Id);
            if (previousConsumer != null)
            {
                freePartition.Consumers?.Remove(previousConsumer);
            }
            freePartition.Consumers?.Add(consumer);
            await context.SaveChangesAsync();
            logger.LogInformation($"Consumer {consumer.Id} " +
                                  $"added to group {consumerGroup.ConsumerGroupName} " +
                                  $"to partition {freePartition.Id}");
            return new ConsumerRegisterResponse
            {
                ConsumerId = consumer.Id,
                ProcessingStatus = ProcessingStatus.Success,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new ConsumerRegisterResponse
            {
                ConsumerId = Guid.Empty,
                ProcessingStatus = ProcessingStatus.Wrong,
                Message = e.Message
            };
        }
    }

    public async Task<ConsumerPollResponse> Poll(Guid consumerId, int batchSize)
    {
        try
        {
            var consumerGroupId = (await context.Consumers
                .FirstAsync(c => c.Id == consumerId)).ConsumerGroupId;

            var consumerGroupOffset = context.ConsumerGroupOffsets
                .Include(cgo => cgo.Partition)
                .Where(cgo => cgo.ConsumerGroupId == consumerGroupId);

            var consumerPartitions = context.Consumers
                .Include(c => c.Partitions)!
                .ThenInclude(p => p.Messages)
                .ThenInclude(m => m.ConsumerGroupMessageStatuses)
                .Where(c => c.Id == consumerId)
                .SelectMany(c => c.Partitions)
                .AsEnumerable();

            if (!consumerPartitions.Any())
            {
                logger.LogError($"No partition found for consumer {consumerId} in group {consumerGroupId}");
                return new ConsumerPollResponse
                {
                    Message = $"No partition found for consumer {consumerId}",
                    ProcessingStatus = ProcessingStatus.Wrong
                };
            }
            
            var partition = consumerPartitions
                .MinBy(p => p.Messages.Count);
            
            var offset = consumerGroupOffset
                .First(cgo => cgo.Partition.Id == partition!.Id).Offset;

            var messages = partition!.Messages
                .OrderBy(m => m.PartitionNumber)
                .Skip(offset)
                .Where(m => !m.IsDeleted && m.ConsumerGroupMessageStatuses
                    .First(cgms => cgms.ConsumerGroupId == consumerGroupId).Status == MessageStatus.Pending)
                .Take(batchSize)
                .ToList();
            
            var validMessages = new List<Message>();
            foreach (var message in messages)
            {
                await context.Entry(message).ReloadAsync();
                var messageStatus = message.ConsumerGroupMessageStatuses
                    .First(cgms => cgms.ConsumerGroupId == consumerGroupId);
                if (messageStatus.Status != MessageStatus.Pending)
                {
                    continue;
                }
                messageStatus.Status = MessageStatus.Processing;
                messageStatus.ConsumerId = consumerId;
                validMessages.Add(message);
            }
            await context.SaveChangesAsync();
            
            var currentOffset = context.ConsumerGroupOffsets
                .First(c => c.ConsumerGroupId == consumerGroupId && 
                            c.PartitionId == partition.Id);
            
            var newOffset = validMessages.FirstOrDefault()?.PartitionNumber ?? currentOffset.Offset;
            var consumerMessages = validMessages
                .Select(m => new ConsumerMessage
                {
                    ValueJson = m.ValueJson
                })
                .ToList();
            logger.LogInformation($"Consumer {consumerId} received {consumerMessages.Count} messages to process", consumerMessages);
            return new ConsumerPollResponse
            {
                ConsumerMessages = consumerMessages,
                Offset = newOffset,
                ProcessingStatus = ProcessingStatus.Success,
                ValueType = validMessages.FirstOrDefault()?.ValueType,
                PartitionId = partition.Id
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new ConsumerPollResponse
            {
                ProcessingStatus = ProcessingStatus.Wrong,
                Message = e.Message
            };
        }
    }

    public async Task<bool> TryCommitMessages(Guid consumerId, ConsumerCommitRequest consumerCommitRequest)
    {
        try
        {
            if (consumerCommitRequest.SuccessProcessedMessagesCount > consumerCommitRequest.BatchSize)
            {
                logger.LogInformation($"Consumer {consumerId} haven't committed messages");
                return false;
            }
            var consumerGroupId = (await context.Consumers
                .FirstAsync(c => c.Id == consumerId)).ConsumerGroupId;
            
            var messages = context.Messages
                .Include(m => m.ConsumerGroupMessageStatuses)
                .Where(m => m.PartitionId == consumerCommitRequest.PartitionId && 
                            m.PartitionNumber >= consumerCommitRequest.Offset &&
                            m.PartitionNumber < consumerCommitRequest.Offset + consumerCommitRequest.BatchSize);
            
            var processedMessages = messages
                .Take(consumerCommitRequest.SuccessProcessedMessagesCount ?? consumerCommitRequest.BatchSize);
            
            var invalidMessages = messages
                .AsEnumerable()
                .Except(processedMessages);

            foreach (var message in invalidMessages)
            {
                message.IsDeleted = true;
            }
            
            foreach (var message in processedMessages)
            {
                var consumerGroupMessageStatus = message.ConsumerGroupMessageStatuses.
                    First(cgmo => cgmo.ConsumerGroupId == consumerGroupId);
                consumerGroupMessageStatus.Status = MessageStatus.Processed;
            }
            
            var currentConsumerGroupOffset = context.ConsumerGroupOffsets
                .First(c => c.ConsumerGroupId == consumerGroupId && 
                                     c.PartitionId == consumerCommitRequest.PartitionId);
            
            if (consumerCommitRequest.Offset >= currentConsumerGroupOffset.Offset)
            {
                currentConsumerGroupOffset.Offset = consumerCommitRequest.Offset + consumerCommitRequest.BatchSize;
            }
            await context.SaveChangesAsync();
            logger.LogInformation($"Consumer {consumerId} successfully committed {consumerCommitRequest.SuccessProcessedMessagesCount} messages");
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}