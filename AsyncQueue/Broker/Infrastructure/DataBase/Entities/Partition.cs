namespace Broker.Infrastructure.DataBase.Entities;

public class Partition
{
    public int PartitionId { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; }
    public List<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
    public List<Message> Messages { get; set; }
}