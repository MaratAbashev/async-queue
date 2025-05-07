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

                foreach (var message in result)
                {
                    await telegramBotService.SendMessageAsync(message.Payload!, stoppingToken);
                    await consumerClient.CommitOffset(message.PartitionId, message.Offset, stoppingToken);
                }
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