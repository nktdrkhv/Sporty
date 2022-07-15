using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Payments;

namespace Sporty.Handlers;

public static class PaymentsHandler
{
    public static async Task OnPaymentPreCheckout(ITelegramBotClient botClient, PreCheckoutQuery preCheckoutQuery)
    {
        Console.WriteLine("PreCheckoutQuery_OnPaymentPreCheckout");
        await Dialogue.Dialogue.HandlePaymentConfirm(botClient, preCheckoutQuery);
    }

    public static async Task OnPaymentSuccess(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine("SuccessMessage_OnPaymentSuccess");
        await Dialogue.Dialogue.HandlePaymentSuccess(botClient, message);
    }
}