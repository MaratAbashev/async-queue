using TelegramConsumer.Abstractions;

namespace TelegramConsumer;

public class Worker(ITelegramBotService telegramBotService, IConsumerService consumerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumerService.RegisterAsync("tgBotGroup", stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await consumerService.PollAsync<string>(stoppingToken);
            if (result == null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }
            
            await consumerService.CommitOffset(result.PartitionId, result.Offset, stoppingToken);
        }
    }
}