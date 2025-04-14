namespace ProducerClient.Models;

internal class BrokerResponse
{
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public Guid? ProducerId { get; set; }
}