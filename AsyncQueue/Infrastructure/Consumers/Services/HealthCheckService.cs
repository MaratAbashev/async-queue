using System.Net.NetworkInformation;
using Domain.Abstractions.Services;

namespace Infrastructure.Consumers.Services;

public class HealthCheckService: IHealthCheckService
{
    public async Task<bool> CheckConsumerHealthAsync(string? healthCheckUrl)
    {
        if (string.IsNullOrEmpty(healthCheckUrl))
        {
            return false;
        }
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(healthCheckUrl);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}