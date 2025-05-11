
using Domain.Models.ConsumersDtos;

namespace Domain.Abstractions.Services;

public interface IConsumerService
{
    Task<ConsumerRegisterResponse> RegisterConsumerAsync(ConsumerRegisterRequest consumerRegisterRequest);
    Task<ConsumerPollResponse> Poll(Guid consumerIdб, int batchSize);
    Task<bool> TryCommitMessages(Guid consumerId, ConsumerCommitRequest consumerCommitRequest);
}