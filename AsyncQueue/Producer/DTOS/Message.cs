namespace Producer.DTOS;

public class Message<TKey,TValue>
{
    public required TKey? Key { get; set; }
    public required TValue Payload { get; set; }
    public required Guid ProducerId { get; set; }
    public required uint Sequence { get; set; }
    public required string Topic { get; set; }
}