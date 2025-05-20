using Domain.Abstractions;

namespace Domain.Entities;

public class Consumer: IEntity<Guid>
{
    public int ConsumerGroupId { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
    public List<Partition>? Partitions { get; set; }
    public List<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public Guid Id { get; set; }
    public string? Address { get; set; }
    public DateTime RegisteredAt { get; set; }
    public bool IsDeleted { get; set; }
}