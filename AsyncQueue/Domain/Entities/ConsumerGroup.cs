using Domain.Abstractions;

namespace Domain.Entities;

public class ConsumerGroup: IEntity<int>
{
    public string? ConsumerGroupName { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; }
    public List<Consumer> Consumers { get; set; }
    public List<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public List<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}