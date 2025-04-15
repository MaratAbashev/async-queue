using Broker.Domain.Models;

namespace Broker.Infrastructure.DataBase.Entities;

public class ConsumerGroupMessageStatus
{
    public int ConsumerGroupMessageStatusId { get; set; }
    public Guid MessageId { get; set; }
    public int ConsumerGroupId { get; set; }
    public MessageStatus Status { get; set; }
    public Message Message { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
}