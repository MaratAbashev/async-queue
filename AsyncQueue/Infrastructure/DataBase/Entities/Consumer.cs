namespace Infrastructure.DataBase.Entities;

public class Consumer
{
    public Guid ConsumerId { get; set; }
    public int ConsumerGroupId { get; set; }
    public ConsumerGroup ConsumerGroup { get; set; }
}