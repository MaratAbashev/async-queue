using Domain.Abstractions;

namespace Domain.Entities;

public class Topic: IEntity<int>
{
    public string? TopicName { get; set; }
    public List<ConsumerGroup> ConsumerGroups { get; set; }
    public List<Partition> Partitions { get; set; }
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}