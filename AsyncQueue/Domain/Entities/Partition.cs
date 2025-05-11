using Domain.Abstractions;

namespace Domain.Entities;

public class Partition: IEntity<int>
{
    public int TopicId { get; set; }
    public Topic Topic { get; set; }
    public List<ConsumerGroupOffset> ConsumerGroupOffsets { get; set; }
    public List<Message> Messages { get; set; }
    public List<Consumer>? Consumers { get; set; }
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}