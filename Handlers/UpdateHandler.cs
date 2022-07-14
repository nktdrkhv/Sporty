using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Stateless;

namespace Sporty.Handlers;

public static class UpdateHandler
{
    public static Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageAsync(botClient, update.Message!),
            UpdateType.CallbackQuery => BotOnCallbackQueryAsync(botClient, update.CallbackQuery!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            Console.WriteLine();
            await handler;
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Необработанное сообщение");
        }
        catch (Exception exception)
        {
            await PollingErrorHandler(botClient, exception, cancellationToken);
        }
    }

    private static async Task BotOnMessageAsync(ITelegramBotClient botClient, Message message)
    {
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/start" => MessageHandler.OnStartCommand(botClient, message),
            "/menu" => MessageHandler.OnMenuCommand(botClient, message),
            _ => MessageHandler.OnTextEnter(botClient, message)
        };

        await action;
    }


    private static async Task BotOnCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var parts = callbackQuery.Data!.Split(' ');
        Task action;
        switch (parts[0])
        {
            case "=":
                callbackQuery.Data = parts[1];
                action = CallbackQueryHandler.OnTextEnter(botClient, callbackQuery);
                break;
            case "*":
                callbackQuery.Data = parts[1];
                action = CallbackQueryHandler.OnDoubleButtonPressed(botClient, callbackQuery);
                break;
            default:
                action = CallbackQueryHandler.OnButtonPressed(botClient, callbackQuery);
                break;
        };

        await action;
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}
