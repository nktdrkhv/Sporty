using System;
using System.ComponentModel;
using AutoMapper;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sporty.Dialogue;

public class DialogueHelper
{
    static DialogueHelper()
    {

    }


    public enum InlineCallBack { Register, Name, Gender, Weight, Age, Email }


    public static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", DialogueHelper.InlineCallBack.Age.ToString()),
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