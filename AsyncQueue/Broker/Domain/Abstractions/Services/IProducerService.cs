using Broker.Domain.Models.ProducersDtos;

namespace Broker.Domain.Abstractions.Services;

public interface IProducerService
{
    public Task<ProducerRegistrationResponse> RegisterAsync(ProducerRegistrationRequest registerRequest, 
        CancellationToken cancellationToken = default);
    public Task<ProducerSendResponse> ProduceAsync(ProducerSendRequest sendRequest, 
        CancellationToken cancellationToken = default);
}