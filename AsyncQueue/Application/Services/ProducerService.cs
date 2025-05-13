using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using Domain.Models.ProducersDtos;

namespace Application.Services;

public class ProducerService(
    IProducerRepository producerRepository,
    IMessageRepository messageRepository,
    ITopicRepository topicRepository,
    IConsumerGroupRepository consumerGroupRepository,
    IConsumerGroupMessageStatusRepository consumerGroupMessageStatusRepository): IProducerService
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
            return new ProducerRegistrationResponse
            {
                Status = ProcessingStatus.Success
            };
        }
        catch (Exception ex)
        {
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
            return new ProducerSendResponse
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Producer unregister"
            };
        }
        var lastSequenceNumber = producer.CurrentSequenceNumber;
        if (sendRequest.Sequence == lastSequenceNumber)
        {
            return new ProducerSendResponse
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Resending the message"
            };
        }

        if (sendRequest.Sequence > lastSequenceNumber + 1)
        {
            return new ProducerSendResponse()
            {
                Status = ProcessingStatus.Wrong,
                Reason = "Some messages are skipped"
            };
        }

        if (sendRequest.Sequence < lastSequenceNumber)
        {
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

        var consumerGroups = await consumerGroupRepository
            .GetAllByFilterAsync(cg => true);

        foreach (var consumerGroup in consumerGroups)
        {
            await consumerGroupMessageStatusRepository.AddAsync(new ConsumerGroupMessageStatus
            {
                ConsumerGroupId = consumerGroup.Id,
                MessageId = messageId,
                Status = MessageStatus.Pending
            });
        }

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