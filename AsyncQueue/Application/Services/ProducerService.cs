using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProducerService(
    IProducerRepository producerRepository,
    IMessageRepository messageRepository,
    ITopicRepository topicRepository,
    IConsumerGroupRepository consumerGroupRepository,
    IConsumerGroupMessageStatusRepository consumerGroupMessageStatusRepository,
    BrokerDbContext context,
    ILogger<ProducerService> logger): IProducerService
{
    public async Task<ProducerRegistrationResponse> RegisterAsync(ProducerRegistrationRequest registerRequest, CancellationToken cancellationToken = default)
    {
        var producer = new Producer
        {
            Id = registerRequest.ProducerId,
            CurrentSequenceNumber = 0
        };
        try
        {
            var producerResponse = await producerRepository.AddAsync(producer);
            logger.LogInformation($"Producer {producer.Id} has been added.");
            return new ProducerRegistrationResponse
            {
                Status = ProcessingStatus.Success
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return new ProducerRegistrationResponse
            {
                Status = ProcessingStatus.Wrong,
                Reason = ex.Message
            };
        }
    }

    public async Task<ProducerSendResponse> ProduceAsync(ProducerSendRequest sendRequest, CancellationToken cancellationToken = default)
    {
        var producer = await producerRepository
            .GetByFilterAsync(p => p.Id == sendRequest.ProducerId);
        if (producer == null)
        {
            logger.LogError($"Producer {sendRequest.ProducerId} has not been registered.");
            return new ProducerSendResponse
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Producer unregister"
            };
        }
        var lastSequenceNumber = producer.CurrentSequenceNumber;
        if (sendRequest.Sequence == lastSequenceNumber)
        {
            logger.LogError($"Producer {sendRequest.ProducerId} has already been registered.");
            return new ProducerSendResponse
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Resending the message"
            };
        }

        if (sendRequest.Sequence > lastSequenceNumber + 1)
        {
            logger.LogError($"Producer {sendRequest.ProducerId} skipped some messages.");
            return new ProducerSendResponse()
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Some messages are skipped"
            };
        }

        if (sendRequest.Sequence < lastSequenceNumber)
        {
            logger.LogError($"Producer {sendRequest.ProducerId} sent previous message");
            return new ProducerSendResponse()
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Sent previous message"
            };
        }
        
        producer.CurrentSequenceNumber = (int)sendRequest.Sequence;
        await producerRepository.UpdateAsync(producer);
        
        var groupMessagesByPartitionId = await topicRepository
            .GroupMessagesInTopicByPartitionAsync(sendRequest.Topic);

        var message = sendRequest.Message;
        var messageId = Guid.NewGuid();
        var partition = GetPartitionByMessage(message.Key, groupMessagesByPartitionId);

        await messageRepository.AddAsync(new Message
        {
            Id = messageId,
            PartitionId = partition.partitionId,
            PartitionNumber = partition.partitionNumber,
            Key = message.Key,
            ValueType = message.ValueType,
            ValueJson = message.ValueJson,
            CreatedAt = DateTime.UtcNow,
        });

        var consumerGroups = await context.ConsumerGroups
            .AsNoTracking()
            .Include(cg => cg.Topic)
            .Where(cg => cg.Topic.TopicName == sendRequest.Topic)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var consumerGroup in consumerGroups)
        {
            await consumerGroupMessageStatusRepository.AddAsync(new ConsumerGroupMessageStatus
            {
                ConsumerGroupId = consumerGroup.Id,
                MessageId = messageId,
                Status = MessageStatus.Pending
            });
        }
        logger.LogInformation($"Message from producer {sendRequest.ProducerId} " +
                              $"has been registered to partition {partition.partitionId} " +
                              $"with sequence {producer.CurrentSequenceNumber} " +
                              $"at partition number {partition.partitionNumber}",
            message.ValueType, message.ValueJson);
        return new ProducerSendResponse
        {
            Status = ProcessingStatus.Success
        };
    }

    private (int partitionId, int partitionNumber) GetPartitionByMessage(
        string? key,
        Dictionary<int, IEnumerable<Message>> groupMessagesByPartitionId)
    {
        int partitionId;
        if (key is not null)
        {
            partitionId = groupMessagesByPartitionId.Keys
                .ElementAt(Math.Abs(key.GetHashCode()) % groupMessagesByPartitionId.Count);
        }
        else
        {
            partitionId = groupMessagesByPartitionId
                .MinBy(pair => pair.Value.Count()).Key;
        }
        var partitionNumber = groupMessagesByPartitionId[partitionId].Count();
        
        return (partitionId, partitionNumber);
    }
}