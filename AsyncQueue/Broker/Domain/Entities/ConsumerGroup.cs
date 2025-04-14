namespace Broker.Domain.Entities;

public class ConsumerGroup
{
    public int ConsumerGroupId { get; set; }
    public string? ConsumerGroupName { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; }
    public List<Consumer> Consumers { get; set; }
    public List<ConsumerGroupMessageStatus> ConsumerGroupMessageStatuses { get; set; }
    public List<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
}