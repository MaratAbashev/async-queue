using ConsumerClient.Abstractions;
using DbConsumer.Abstractions;

namespace DbConsumer;

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
                foreach (var message in result.MessagesPayload)
                {
                    await consumerRepository.AddMessageAsync(message!, stoppingToken);
                }
                await consumerClient.CommitOffset(result.PartitionId, result.Offset, result.MessagesPayload.Count, result.MessagesPayload.Count, stoppingToken);
                await Task.Delay(2000, stoppingToken);
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