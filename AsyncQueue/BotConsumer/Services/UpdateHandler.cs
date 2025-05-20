using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BotConsumer.Services;

public class UpdateHandler: IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        Console.WriteLine($"Sender: {message!.From!.Username}");
        Console.WriteLine($"Message: {message.Text}");
        Console.WriteLine();
        await botClient.SendMessage(message.Chat.Id, $"Айди чата: {message.Chat.Id}. Вас понял, {message.From.FirstName} {message.From.LastName} @{message.From.Username}", cancellationToken: cancellationToken);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}