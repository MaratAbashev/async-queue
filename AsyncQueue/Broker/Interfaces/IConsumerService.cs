using Broker.Controllers;

namespace Broker.Interfaces;

public interface IConsumerService
{
    public ResponseMessageDto Poll(Guid consumerId);
    public bool Register(string consumerGroup, Guid consumerId);
    public ResponseMessageDto Consume(Guid consumerId); // другая дто
}