namespace Domain.Models.ConsumersDtos;

public class ConsumerMessage
{
    public int PartitionId { get; set; }
    public long Offset { get; set; }
    public string ValueJson { get; set; }
    public string ValueType { get; set; }
}