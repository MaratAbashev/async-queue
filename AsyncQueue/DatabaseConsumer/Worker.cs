using ConsumerClient.Abstractions;
using DatabaseConsumer.Abstractions;
using DatabaseConsumer.Services.Database;

namespace DatabaseConsumer;

public class Worker(IDbConsumerRepository consumerRepository, IConsumerClient<string> consumerClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumerClient.Register(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await consumerClient.Poll(stoppingToken);
                if (result == null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                foreach (var message in result)
                {
                    await consumerRepository.AddMessageAsync(message.Payload!, stoppingToken);
                    await consumerClient.CommitOffset(message.PartitionId, message.Offset, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}