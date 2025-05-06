namespace Domain.Models.ConsumersDtos;

public class ConsumerRegisterResponse
{
    public Guid ConsumerId { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string? Message { get; set; }
}