using DatabaseConsumer.Abstractions;
using DatabaseConsumer.Services.Database;

namespace DatabaseConsumer;

public class Worker(IConsumerService<string> consumerService, IDbConsumerRepository consumerRepository) : BackgroundService
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
            await consumerRepository.AddMessageAsync(result.Value, stoppingToken);
            await consumerService.CommitOffset(result.PartitionId, result.Offset, stoppingToken);
        }
    }
}