namespace Domain.Models.ProducersDtos;

public class ProducerRegistrationResponse
{
    public ProcessingStatus Status { get; set; }
    public string? Reason { get; set; }
}