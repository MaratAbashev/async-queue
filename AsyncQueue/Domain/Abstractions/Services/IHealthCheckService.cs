namespace Domain.Abstractions.Services;

public interface IHealthCheckService
{
    Task<bool> CheckConsumerHealthAsync(string? healthCheckUrl);
}