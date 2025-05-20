using System.Net.NetworkInformation;
using Domain.Abstractions.Services;

namespace Infrastructure.Consumers.Services;

public class HealthCheckService(IHttpClientFactory factory): IHealthCheckService
{
    public async Task<bool> CheckConsumerHealthAsync(string? hostName)
    {
        if (string.IsNullOrEmpty(hostName))
        {
            return false;
        }
        try
        {
            var client = factory.CreateClient(hostName);
            var response = await client.GetAsync("/health-check");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}