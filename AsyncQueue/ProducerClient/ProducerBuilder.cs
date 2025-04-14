using ProducerClient.Abstractions;

namespace ProducerClient;

public class ProducerBuilder<TKey, TValue>(string brokerUrl): IProducerBuilder<TKey, TValue>
{
    public IProducer<TKey, TValue> Build()
    {
        return new Producer<TKey, TValue>(brokerUrl);
    }
}