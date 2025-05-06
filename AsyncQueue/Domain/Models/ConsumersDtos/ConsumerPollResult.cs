namespace Domain.Models.ConsumersDtos;

public class ConsumerPollResult<T>
{
    public int PartitionId { get; set; }
    public long Offset { get; set; }
    public T? Payload { get; set; }
}

