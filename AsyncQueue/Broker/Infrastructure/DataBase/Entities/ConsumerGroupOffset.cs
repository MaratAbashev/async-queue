namespace Broker.Infrastructure.DataBase.Entities;

public class ConsumerGroupOffset
{
    public int ConsumerGroupOffsetId { get; set; }
    public int PartitionId { get; set; }
    public int ConsumerGroupId { get; set; }
    public int Offset { get; set; }
    public Partition Partition { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
}