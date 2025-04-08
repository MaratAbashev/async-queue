using Broker.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Interfaces;

public interface IProducerService
{
    public ResponseMessageDto Produce(RequestMessageDto requestMessageDto);
    public ResponseMessageDto GetResult(Guid messageId);
}