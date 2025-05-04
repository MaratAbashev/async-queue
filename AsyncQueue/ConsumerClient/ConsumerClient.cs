using System.Net.Http.Json;
using System.Text.Json;
using Domain.Models.ConsumersDtos;

namespace ConsumerClient;

public class ConsumerClient<T>: IDisposable
{
    private Guid _consumerId;
    private bool _isRegistered;
    private readonly HttpClient _client;
    public string ConsumerGroup { get; }
    public ConsumerClient(string consumerGroup, string brokerUrl)
    {
        ConsumerGroup = consumerGroup;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(brokerUrl);
    }

    public async Task Register(CancellationToken cancellationToken = default)
    {
        EnsureRegistered();
        var registerRequest = new ConsumerRegisterRequest
        {
            ConsumerGroup = ConsumerGroup
        };
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("/consumer/register",
                    registerRequest, cancellationToken); //check timeouts/heartbeat
                var content = await result.EnsureSuccessStatusCode().Content
                    .ReadFromJsonAsync<ConsumerRegisterResponse>(cancellationToken: cancellationToken);
                if (content == null)
                    throw new NullReferenceException("No consumer id in register response");
                _isRegistered = true;
                _consumerId = content.ConsumerId;
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public async Task<ConsumerPollResult<T>?> Poll(CancellationToken cancellationToken = default)
    {
        EnsureRegistered();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.GetAsync($"/consumer/{_consumerId}/poll", cancellationToken);
                if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return null;
                var consumerMessage = await result.EnsureSuccessStatusCode().Content
                    .ReadFromJsonAsync<ConsumerMessage>(cancellationToken: cancellationToken);
                if (consumerMessage == null)
                    throw new NullReferenceException("Consumer message is null");
                if (typeof(T).Name != consumerMessage.ValueType) // or check type fullname
                    throw new FormatException("Consumer message is not of type " + consumerMessage.ValueType);
                return new ConsumerPollResult<T>
                {
                    PartitionId = consumerMessage.PartitionId,
                    Offset = consumerMessage.Offset,
                    Payload = JsonSerializer.Deserialize<T>(consumerMessage.ValueJson)
                };
            }
            catch (Exception ex) when(ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
        }
        return null;
    }

    public async Task CommitOffset(int partitionId, long offset, CancellationToken cancellationToken = default)
    {
        EnsureRegistered();
        var commitRequest = new ConsumerCommitRequest
        {
            PartitionId = partitionId,
            Offset = offset
        };
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"/consumer/{_consumerId}/commit", commitRequest, cancellationToken);
                result.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private void EnsureRegistered()
    {
        if (_isRegistered)
            throw new InvalidOperationException("ConsumerClient is already registered");
    }
    
    public void Dispose()
    {
        _client.Dispose();
    }
}