namespace ProducerClient.Models;

public class Message<TKey, TValue>
{
    public TKey? Key { get; set; }
    public required TValue Payload { get; set; }
}