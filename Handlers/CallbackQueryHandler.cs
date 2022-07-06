using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Sporty.Dialogue;

namespace Sporty.Handlers;

public static class CallbackQueryHandler
{
    public static async Task<Message> OnButtonPressed(ITelegramBotClient botClient, CallbackQuery callbackQuery) => await Dialogue.Dialogue.HandleCallbackQueryAsync(callbackQuery);
}