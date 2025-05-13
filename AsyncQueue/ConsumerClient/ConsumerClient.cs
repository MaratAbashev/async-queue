using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using ConsumerClient.Abstractions;
using Domain.Endpoints;
using Domain.Models;
using Domain.Models.ConsumersDtos;

namespace ConsumerClient;

public class ConsumerClient<T>: IConsumerClient<T>
{
    private readonly HttpClient _client;
    private Guid _consumerId;
    private bool _isRegistered;
    private int _batchSize;
    public string ConsumerGroup { get; }
    public ConsumerClient(string consumerGroup, string brokerUrl, int batchSize)
    {
        ConsumerGroup = consumerGroup;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(brokerUrl);
        _batchSize = batchSize;
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
                var result = await _client.PostAsJsonAsync($"/{ConsumerEndpoints.Register}",
                    registerRequest, cancellationToken); //check timeouts/heartbeat
                var content = await result.EnsureSuccessStatusCode().Content
                    .ReadFromJsonAsync<ConsumerRegisterResponse>(cancellationToken: cancellationToken);
                if (content == null)
                    throw new NullReferenceException("No consumer id in register response");
                if (content.ProcessingStatus == ProcessingStatus.Wrong)
                    throw new InvalidOperationException(content.Message);
                _isRegistered = true;
                _consumerId = content.ConsumerId;
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(5000, cancellationToken).WaitAsync(cancellationToken);
            }
        }
    }

    public async Task<ConsumerPollResult<T>?> Poll(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.GetAsync($"consumer/{_consumerId}/{_batchSize}/{ConsumerEndpoints.Poll}", cancellationToken);
                if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return null;
                
                var consumerPollResponse = await result.EnsureSuccessStatusCode().Content
                    .ReadFromJsonAsync<ConsumerPollResponse>(cancellationToken: cancellationToken);
                if (consumerPollResponse == null)
                    throw new NullReferenceException("Consumer response is null");
                if (consumerPollResponse.ProcessingStatus == ProcessingStatus.Wrong)
                    throw new InvalidOperationException(consumerPollResponse.Message);
                if (consumerPollResponse.ConsumerMessages == null || consumerPollResponse.ConsumerMessages.Count == 0)
                    throw new ArgumentException("Consumer response is empty");
                if (typeof(T).Name != consumerPollResponse.ValueType) // or check type fullname
                    throw new InvalidDataContractException("Consumer message is not of type " + consumerPollResponse.ValueType);
                return new ConsumerPollResult<T>
                {
                    Offset = consumerPollResponse.Offset,
                    PartitionId = consumerPollResponse.PartitionId,
                    MessagesPayload = consumerPollResponse.ConsumerMessages
                        .Select(cm => JsonSerializer.Deserialize<T>(cm.ValueJson))
                        .ToList()
                };
            }
            catch (Exception ex) when(ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(5000, cancellationToken).WaitAsync(cancellationToken);
            }
        }
        return null;
    }

    public async Task CommitOffset(int partitionId, int offset, int batchSize, int? successMessagesCount, CancellationToken cancellationToken = default)
    {
        var commitRequest = new ConsumerCommitRequest
        {
            PartitionId = partitionId,
            Offset = offset,
            BatchSize = batchSize,
            SuccessProcessedMessagesCount = successMessagesCount
        };
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _client.PostAsJsonAsync($"consumer/{_consumerId}/{ConsumerEndpoints.CommitOffset}", commitRequest, cancellationToken);
                result.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(5000, cancellationToken).WaitAsync(cancellationToken);
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