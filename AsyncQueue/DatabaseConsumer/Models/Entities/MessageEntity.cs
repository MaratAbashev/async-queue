namespace DatabaseConsumer.Models.Entities;

public class MessageEntity
{
    public int MessageId { get; set; }
    public string? Content { get; set; }
    public DateTime Date { get; set; }
}