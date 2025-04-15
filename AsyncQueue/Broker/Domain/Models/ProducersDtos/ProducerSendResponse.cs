namespace Broker.Domain.Models.ProducersDtos;

public class ProducerSendResponse
{
    public ProcessingStatus Status { get; set; }
    public string? Reason { get; set; }
}