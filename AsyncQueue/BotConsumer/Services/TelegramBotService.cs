﻿using BotConsumer.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace BotConsumer.Services;

public class TelegramBotService(ITelegramBotClient client): ITelegramBotService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message
            ],
            DropPendingUpdates = true
        };
        client.StartReceiving<UpdateHandler>(receiverOptions, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(string text, CancellationToken cancellationToken)
    {
        await client.SendMessage(1840413780, text, cancellationToken: cancellationToken); // тут айди моего с ним чата
        await client.SendMessage(1126667142, text, cancellationToken: cancellationToken);
    }
}