using TelegramConsumer.Abstractions;

namespace TelegramConsumer;

public class Worker(ITelegramBotService telegramBotService, IConsumerService<string> consumerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumerService.RegisterAsync("tgBotGroup", stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await consumerService.PollAsync(stoppingToken);
            if (result == null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }
            await telegramBotService.SendMessageAsync(result.Value, stoppingToken);
            await consumerService.CommitOffset(result.PartitionId, result.Offset, stoppingToken);
        }
    }
}