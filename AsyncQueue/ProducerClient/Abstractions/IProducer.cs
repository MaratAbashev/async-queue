using ProducerClient.Models;

namespace ProducerClient.Abstractions;

public interface IProducer<TKey, TValue>
{
    string BrokerUrl { get; }
    Task SendAsync(string topicName, Message<TKey, TValue> message, CancellationToken cancellationToken);
}