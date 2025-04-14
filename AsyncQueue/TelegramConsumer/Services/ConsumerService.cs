using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TelegramConsumer.Abstractions;
using TelegramConsumer.Models;

namespace TelegramConsumer.Services;

public class ConsumerService<T>(HttpClient httpClient) : IConsumerService<T>
{
    private Guid _consumerId;
    private bool _isRegistered;

    public async Task RegisterAsync(string consumerGroup, CancellationToken cancellationToken = default)
    {
        if (_isRegistered)
            return;
        while (true)
        {
            var response = await httpClient.PostAsync($"/broker/register?consumerGroup={consumerGroup}", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            _consumerId = result!.ConsumerId;
            _isRegistered = true;
            break;
        }
    }

    public async Task<MessageResult<T>?> PollAsync(CancellationToken cancellationToken = default)
    {
        EnsureRegistered();
        var response = await httpClient.GetAsync(
            $"/broker/poll?consumerId={_consumerId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Polling failed: {response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<MessageResult<T>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
            cancellationToken);
    }

    public async Task CommitOffset(int partitionId, long offset, CancellationToken cancellationToken = default)
    {
        EnsureRegistered();
        var response = await httpClient.PostAsJsonAsync($"/broker/commit?consumerId={_consumerId}", new CommitContext(partitionId, offset), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Committing offset failed: {response.StatusCode}"); // пересмотреть
    }
    
    private void EnsureRegistered()
    {
        if (!_isRegistered)
            throw new InvalidOperationException("Consumer is not registered");
    }
}