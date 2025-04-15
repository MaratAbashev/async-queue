namespace Broker.Infrastructure.DataBase.Entities;

public class Message
{
    public Guid MessageId { get; set; }
    public int PartitionId { get; set; }
    public string Value { get; set; }
    public int PartitionNumber { get; set; }
    public Partition Partition { get; set; }
    public List<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
}