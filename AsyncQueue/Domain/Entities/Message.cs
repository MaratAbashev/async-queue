using Domain.Abstractions;

namespace Domain.Entities;

public class Message: IEntity<Guid>
{
    public int PartitionId { get; set; }
    public string KeyJson { get; set; }
    public string KeyType { get; set; }
    public string ValueJson { get; set; }
    public string ValueType { get; set; }
    public int PartitionNumber { get; set; }
    public Partition Partition { get; set; }
    public List<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
}