using System;
using System.ComponentModel;

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

    public static string WelcomeText = "Приветсвенное сообщение";
    public static string RegisterText = "Для начала вам необходимо зарегистрироваться";
    public static string RegSeqNameText = "Укажите имя";
    public static string RegSeqGenderText = "Укажите пол";
    public static string RegSeqAgeText = "Укажите возраст";
    public static string RegSeqHeightText = "Укажите рост";
    public static string RegSeqWeightText = "Укажите вес";
    public static string RegSeqEmailText = "Укажите электронную почту";
    public static String RegSeqUnknownText = "Произошла ошибка";

    public static InlineKeyboardMarkup RegisterIkm;
    public static InlineKeyboardMarkup GenderIkm;

    static DialogueHelper()
    {
        RegisterIkm = new(
            new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Регистрация", DialogueTrigger.SignUp.ToString())
                }
            });
        GenderIkm = new(
            new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("М", DialogueTrigger.MaleInput.ToString()),
                    InlineKeyboardButton.WithCallbackData("Ж", DialogueTrigger.FemaleInput.ToString())
                }
            });
    }
}