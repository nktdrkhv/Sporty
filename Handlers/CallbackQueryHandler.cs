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
        await Dialogue.Dialogue.HandleTriggerAsync(botClient, callbackQuery);
    }

    public static async Task OnDoubleButtonPressed(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        Console.WriteLine("Callback_OnDoubleButtonPressed");
        await Dialogue.Dialogue.HandleTriggerAsync(botClient, callbackQuery);
    }

    public static async Task OnTextEnter(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        Console.WriteLine("Callback_OnText");
        await Dialogue.Dialogue.HandleTextAsync(botClient, callbackQuery);
    }
}