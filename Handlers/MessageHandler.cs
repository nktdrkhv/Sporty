using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sporty.Handlers;

public static class MessageHandler
{
    public static async Task OnStartCommand(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("Message_OnStartCommand");
        await Dialogue.Dialogue.HandleStartCommandAsync(botClient, message);
    }

    public static async Task OnMenuCommand(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("Message_OnMenuCommand");
        await Dialogue.Dialogue.HandleMenuCommandAsync(botClient, message);
    }

    public static async Task OnTextEnter(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("Message_OnText");
        await Dialogue.Dialogue.HandleTextAsync(botClient, message);
    }
}

