using ConsumerClient.Abstractions;
using TelegramConsumer.Abstractions;

namespace TelegramConsumer;

public class Worker(ITelegramBotService telegramBotService, IConsumerClient<string> consumerClient) : BackgroundService
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

                await telegramBotService.SendMessageAsync(result.Payload!, stoppingToken);
                await consumerClient.CommitOffset(result.PartitionId, result.Offset, stoppingToken);
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