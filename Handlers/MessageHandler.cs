using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Sporty.Dialogue;

namespace Sporty.Handlers;

public static class MessageHandler
{
    public static async Task<Message> OnStartCommand(ITelegramBotClient botClient, Message message)
    {
        return null;
    }

    public static async Task<Message> OnMenuCommand(ITelegramBotClient botClient, Message message)
    {
        return null;
    }

    public static async Task<Message> OnTextEnter(ITelegramBotClient botClient, Message message) => await Dialogue.Dialogue.HandleTextInputAsync(message);
}

