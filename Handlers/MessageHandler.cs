using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Sporty.Dialogue;

namespace Sporty.Handlers;

public static class MessageHandler
{
    public static async Task OnStartCommand(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("Message_OnStartCommand");
        await Dialogue.Dialogue.HandleStartAsync(botClient, message);
    }

    public static async Task OnMenuCommand(ITelegramBotClient botClient, Message message)
    {
    }

    public static async Task OnTextEnter(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("Message_OnText");
        await Dialogue.Dialogue.HandleTextInputAsync(botClient, message);
    }
}

