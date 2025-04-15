namespace Broker.Infrastructure.DataBase.Entities;

public class Topic
{
    public int TopicId { get; set; }
    public string? TopicName { get; set; }
    public List<ConsumerGroup> ConsumerGroups { get; set; }
    public List<Partition> Partitions { get; set; }
}