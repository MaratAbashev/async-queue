namespace ProducerClient.Models;

internal class MessageRequest<TKey,TValue>
{
    public required Message<TKey,TValue> Message { get; set; }
    public required Guid ProducerId { get; set; }
    public required uint Sequence { get; set; }
    public required string Topic { get; set; }
}