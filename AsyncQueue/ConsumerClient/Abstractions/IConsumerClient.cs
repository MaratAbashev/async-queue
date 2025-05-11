using Domain.Models.ConsumersDtos;

namespace ConsumerClient.Abstractions;

public interface IConsumerClient<T> : IDisposable
{
    string ConsumerGroup { get; }
    Task Register(CancellationToken cancellationToken = default);
    Task<ConsumerPollResult<T>?> Poll(CancellationToken cancellationToken = default);
    Task CommitOffset(int partitionId, long offset, CancellationToken cancellationToken = default);
}