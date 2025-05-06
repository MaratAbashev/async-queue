
using Domain.Models.ConsumersDtos;

namespace Domain.Abstractions.Services;

public interface IConsumerService
{
    Task<ConsumerRegisterResponse> RegisterConsumerAsync(ConsumerRegisterRequest consumerRegisterRequest);
    Task<ConsumerPollResponse> Poll(Guid consumerId);
    Task<bool> TryCommitMessages(Guid consumerId, ConsumerCommitRequest consumerCommitRequest);
}