using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Domain.Models.ConsumersDtos;

namespace Application.Services;

public class ConsumerService(IConsumerRepository consumerRepository,
    IConsumerGroupRepository consumerGroupRepository,
    IConsumerGroupOffsetRepository consumerGroupOffsetRepository,
    IConsumerGroupMessageStatusRepository consumerGroupMessageStatusRepository) : Domain.Abstractions.Services.IConsumerService
{
    public async Task<ConsumerRegisterResponse> RegisterConsumerAsync(ConsumerRegisterRequest consumerRegisterRequest)
    {
        var consumerGroup = await consumerGroupRepository.GetByFilterAsync(cg =>
            cg.ConsumerGroupName == consumerRegisterRequest.ConsumerGroup);
        if (consumerGroup == null)
        {
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
            ConsumerGroup = consumerGroup,
            ConsumerGroupId = consumerGroup.Id
        };
        try
        {
            // TODO реализовать механизм перераспределения партиций между консьюмерами
            await consumerRepository.AddAsync(consumer);
            return new ConsumerRegisterResponse
            {
                ConsumerId = consumer.Id,
                ProcessingStatus = ProcessingStatus.Success
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ConsumerRegisterResponse
            {
                ConsumerId = Guid.Empty,
                ProcessingStatus = ProcessingStatus.Wrong,
                Message = e.Message
            };
        }
    }

    public async Task<ConsumerPollResponse> Poll(Guid consumerId)
    {
        try
        {
            var messages = await GetPendingMessagesByConsumerIdAsync(consumerId);
            var valueType = messages.FirstOrDefault()?.ValueType;
            return new ConsumerPollResponse
            {
                ProcessingStatus = ProcessingStatus.Success,
                ConsumerMessages = messages,
                ValueType = valueType
            };
        }
        catch (Exception e)
        {
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

            var consumer = await consumerRepository.GetByFilterAsync(c => c.Id == consumerId);
            if (consumer == null)
                throw new KeyNotFoundException($"Consumer with id {consumerId} not found");
            var consumerGroupId = consumer.ConsumerGroupId;
            var processingStatuses = await consumerGroupMessageStatusRepository
                .GetAllByStatusAndConsumerGroupIdAsync(consumerGroupId, MessageStatus.Processing);
            var messagesToCommit = processingStatuses
                .Where(cgms => cgms.Message.PartitionId == consumerCommitRequest.PartitionId);
            await consumerGroupMessageStatusRepository
                .UpdateConsumerGroupMessageStatusAsync(messagesToCommit, MessageStatus.Processing);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
    
    private async Task<List<ConsumerMessage>> GetPendingMessagesByConsumerIdAsync(Guid consumerId)
    {
        var consumer = await consumerRepository.GetByFilterAsync(c => c.Id == consumerId);
        if (consumer == null)
            throw new KeyNotFoundException($"Consumer with id {consumerId} not found");
        var consumerGroupId = consumer.ConsumerGroupId;
        var offsets = await consumerGroupOffsetRepository.GetByConsumerGroupIdAsync(consumerGroupId);
        var pendingStatuses = await consumerGroupMessageStatusRepository
            .GetAllByStatusAndConsumerGroupIdAsync(consumerGroupId, MessageStatus.Pending);
        var messages = pendingStatuses.Select(status => new ConsumerMessage
        {
            PartitionId = status.Message.PartitionId,
            ValueType = status.Message.ValueType,
            ValueJson = status.Message.ValueJson,
            Offset = offsets.TryGetValue(status.Message.PartitionId, out var offset)
                ? offset
                : throw new InvalidOperationException("Offset not found for partition")
        }).ToList();
        await consumerGroupMessageStatusRepository
            .UpdateConsumerGroupMessageStatusAsync(pendingStatuses,
                MessageStatus.Processing);
        return messages;
    }
}