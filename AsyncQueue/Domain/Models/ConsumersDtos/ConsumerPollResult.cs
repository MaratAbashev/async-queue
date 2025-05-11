namespace Domain.Models.ConsumersDtos;

public class ConsumerPollResult<T>
{
    public int PartitionId { get; set; }
    public int Offset { get; set; }
    public List<T?> MessagesPayload { get; set; }
}

