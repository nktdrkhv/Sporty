using System;
using System.ComponentModel;
using AutoMapper;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sporty.Dialogue;

public static class DialogueHelper
{
    public static DialogueTrigger ToDialogueTrigger(this string str) => Enum.Parse<DialogueTrigger>(str);
    public static DialogueState ToDialogueState(this string str) => Enum.Parse<DialogueState>(str);

    // --------------------------------------------------------------------------------

    public static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", DialogueTrigger.EmailChange.ToString()),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
            });

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                    text: "Choose",
                                                    replyMarkup: inlineKeyboard);
    }
}