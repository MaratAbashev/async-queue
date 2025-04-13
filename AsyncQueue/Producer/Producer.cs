using System.Text;
using System.Text.Json;
using Producer.DTOS;

namespace Producer;

public class Producer: IDisposable
{
    private const string RegisterEndpoint = "";
    private const string SendEndpoint = "";
    private const string BrokerResponseSuccessValue = "";
    
    private readonly HttpClient _httpClient;
    private readonly string _brokerUrl;
    
    private uint _sequenceNumber = 0;
    private bool _isRegistered = false;
    private Guid _producerId;
    public Producer(string brokerUrl)
    {
        _brokerUrl = brokerUrl;
        _httpClient = new HttpClient {BaseAddress = new Uri(_brokerUrl)};
    }

    public async Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        if (_isRegistered)
        {
            throw new InvalidOperationException("Already registered");
        }
        var registerDto = new ProducerRegistration {ProducerId = _producerId};
        var registerRequest = new StringContent(
            JsonSerializer.Serialize(registerDto),
            Encoding.UTF8,
            "application/json");
        try
        {
            var result = await _httpClient.PostAsync(
                $"/{RegisterEndpoint}",
                registerRequest,
                cancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"RegisterAsync failed with http status code {result.StatusCode}");
            }
            var responseBody = await result.Content.ReadAsStringAsync(cancellationToken);
            var brokerResponse = JsonSerializer.Deserialize<BrokerResponse>(responseBody);

            if (brokerResponse == null)
            {
                throw new Exception("Broker response is null");
            }

            if (brokerResponse.Status != BrokerResponseSuccessValue)
            {
                throw new Exception($"Broker response failed with status: {brokerResponse.Status}\n" +
                                    $"Reason: {brokerResponse.Reason}");
            }
            _producerId = brokerResponse.ProducerId ?? throw new Exception("Broker response producer id is null");
            _isRegistered = true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            //retry
        }
    }

    public async Task SendAsync<TValue, TKey>(string topic, TKey? key, TValue payload,
        CancellationToken cancellationToken = default)
    {
        if (!_isRegistered)
        {
            throw new InvalidOperationException($"Producer {_producerId} has not been registered");
        }
        Interlocked.Increment(ref _sequenceNumber);
        var message = new Message<TKey,TValue>
        {
            Key = key,
            Payload = payload,
            ProducerId = _producerId,
            Sequence = _sequenceNumber,
            Topic = topic,
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var messageRequest = new StringContent(messageJson, Encoding.UTF8, "application/json");

        try
        {
            var result = await _httpClient.PostAsync(
                $"/{SendEndpoint}",
                messageRequest,
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