using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Sporty.Dialogue;

namespace Sporty.Handlers;

public static class CallbackQueryHandler
{
    public static async Task OnButtonPressed(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        Console.WriteLine("Callback_OnButtonPressed");
        await Dialogue.Dialogue.HandleCallbackQueryAsync(botClient, callbackQuery);
    }
}