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
            Id = registerRequest.ProducerId
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
        var partition = GetPartitionByMessage(message, groupMessagesByPartitionId);

        await messageRepository.AddAsync(new Message
        {
            Id = messageId,
            PartitionId = partition.partitionId,
            PartitionNumber = partition.partitionNumber,
            KeyJson = message.KeyJson,
            KeyType = message.KeyType,
            ValueJson = message.ValueJson,
            ValueType = message.ValueType,
        });

        var consumerGroups = await consumerGroupRepository
            .GetAllByFilterAsync(cg => true);

        foreach (var consumerGroup in consumerGroups)
        {
            await consumerGroupMessageStatusRepository.AddAsync(new ConsumerGroupMessageStatus
            {
                ConsumerGroupId = consumerGroup.Id,
                MessageId = messageId
            });
        }

        return new ProducerSendResponse
        {
            Status = ProcessingStatus.Success
        };
    }

    private (int partitionId, int partitionNumber) GetPartitionByMessage(
        ProducerMessage produceMessage,
        Dictionary<int, IEnumerable<Message>> groupMessagesByPartitionId)
    {
        var keys = groupMessagesByPartitionId.Keys.ToList();
        if (keys.Count == 0)
        {
            throw new ArgumentException("There are no partitions");
        }
        var randomIndex = new Random().Next(0, keys.Count - 1);
        return (keys[randomIndex], groupMessagesByPartitionId[keys[randomIndex]].Count());
    }
}