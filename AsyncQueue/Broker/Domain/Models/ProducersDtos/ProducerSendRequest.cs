namespace Broker.Domain.Models.ProducersDtos;

public class ProducerSendRequest
{
    public required Message Message { get; set; }
    public required Guid ProducerId { get; set; }
    public required uint Sequence { get; set; }
    public required string Topic { get; set; }
}