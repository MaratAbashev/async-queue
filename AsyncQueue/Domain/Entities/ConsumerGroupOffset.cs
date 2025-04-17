using Domain.Abstractions;

namespace Domain.Entities;

public class ConsumerGroupOffset: IEntity<int>
{
    public int PartitionId { get; set; }
    public int ConsumerGroupId { get; set; }
    public int Offset { get; set; }
    public Partition Partition { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}