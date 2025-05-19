namespace Domain.Models.ConsumersDtos;

public class ConsumerRegisterRequest
{
    public string ConsumerGroup { get; set; }
    public string? Address { get; set; }
}