using ProducerClient.Models;

namespace ProducerClient.Abstractions;

public interface IProducer<TKey, TValue>
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task SendAsync(string topicName, Message<TKey, TValue> message, CancellationToken cancellationToken);
}