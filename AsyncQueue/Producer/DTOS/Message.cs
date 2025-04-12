namespace Producer.DTOS;

public class Message
{
    public required Guid ProducerId { get; set; }
    public required object Payload { get; set; }
    public required long Sequence { get; set; }
}