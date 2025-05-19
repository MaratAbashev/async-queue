using Domain.Abstractions;
using Domain.Models;

namespace Domain.Entities;

public class ConsumerGroupMessageStatus: IEntity<long>
{
    public Guid MessageId { get; set; }
    public int ConsumerGroupId { get; set; }
    public Guid ConsumerId { get; set; }
    public MessageStatus Status { get; set; }
    public Message Message { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
    public Consumer Consumer { get; set; }
    public long Id { get; set; }
    public bool IsDeleted { get; set; }
}