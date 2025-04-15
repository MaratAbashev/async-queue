using Broker.Controllers;

namespace Broker.Domain.Abstractions.Services;

public interface IConsumerService
{
    public ResponseMessageDto Poll(Guid consumerId);
    public bool Register(string consumerGroup);
    public ResponseMessageDto Consume(Guid consumerId); // другая дто
}