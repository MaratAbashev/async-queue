using TelegramConsumer.Models;

namespace TelegramConsumer.Abstractions;

public interface IConsumerService<T>
{
    Task RegisterAsync(string consumerGroup, CancellationToken cancellationToken = default);
    Task<MessageResult<T>?> PollAsync(CancellationToken cancellationToken = default);
    Task CommitOffset(int partitionId, long offset, CancellationToken cancellationToken = default);
}