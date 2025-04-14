using System.Text;
using System.Text.Json;
using ProducerClient.Abstractions;
using ProducerClient.Models;

namespace ProducerClient;

internal class Producer<TKey, TValue>: IProducer<TKey, TValue>, IDisposable
{
    private const string SendEndpoint = "";
    private const string BrokerResponseSuccessValue = "";
    
    private readonly Guid _producerId = Guid.NewGuid();
    private readonly HttpClient _httpClient;
    
    private uint _sequenceNumber = 0;
    
    public string BrokerUrl { get; }
    
    public Producer(string brokerUrl)
    {
        BrokerUrl = brokerUrl;
        _httpClient = new HttpClient {BaseAddress = new Uri(BrokerUrl)};
    }
    
    public async Task SendAsync(string topic, Message<TKey, TValue> message,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _sequenceNumber);
        var messageRequest = new MessageRequest<TKey,TValue>
        {
            Message = message,
            ProducerId = _producerId,
            Sequence = _sequenceNumber,
            Topic = topic,
        };
        
        var messageJson = JsonSerializer.Serialize(messageRequest);
        var messageRequestContent = new StringContent(messageJson, Encoding.UTF8, "application/json");

        try
        {
            var result = await _httpClient.PostAsync(
                $"/{SendEndpoint}",
                messageRequestContent,
                cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var responseBody = await result.Content.ReadAsStringAsync(cancellationToken);
                var brokerResponse = JsonSerializer.Deserialize<BrokerResponse>(responseBody);
                if (brokerResponse != null && brokerResponse.Status == BrokerResponseSuccessValue)
                {
                    return;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        Interlocked.Decrement(ref _sequenceNumber);
        throw new Exception("Failed to send message");
    }
    
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}