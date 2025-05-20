namespace BotConsumer.Abstractions;

public interface ITelegramBotService
{
    public Task StartAsync(CancellationToken cancellationToken);
    public Task SendMessageAsync(string text, CancellationToken cancellationToken);
}