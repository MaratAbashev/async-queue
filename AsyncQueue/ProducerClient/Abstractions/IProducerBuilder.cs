namespace ProducerClient.Abstractions;

public interface IProducerBuilder<TKey, TValue>
{
    IProducer<TKey, TValue> Build();
}