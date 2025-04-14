using ProducerClient.Models;

namespace ProducerClient.Abstractions;

public interface IProducer<TKey, TValue>
{
    string BrokerUrl { get; }
    Task RegisterAsync(CancellationToken cancellationToken = default);
    Task SendAsync(string topicName, Message<TKey, TValue> message, CancellationToken cancellationToken = default);
}