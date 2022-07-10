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

    public static string WelcomeText = "<i>Приветсвенное сообщение</i>";
    public static string RegisterText = "Для начала вам необходимо <b>зарегистрироваться</b>";
    public static string RegSeqNameText = "Укажите имя";
    public static string RegSeqGenderText = "Укажите пол";
    public static string RegSeqAgeText = "Укажите возраст";
    public static string RegSeqHeightText = "Укажите рост";
    public static string RegSeqWeightText = "Укажите вес";
    public static string RegSeqEmailText = "Укажите электронную почту";
    public static string RegSeqUnknownText = "Произошла ошибка";
    public static string MenuText(string name) => $"<b>Здравствуйте, {name}!</b>\n";

    public static InlineKeyboardMarkup RegisterIkm;
    public static InlineKeyboardMarkup GenderIkm;
    public static InlineKeyboardMarkup MenuIkm;

    static DialogueHelper()
    {
        RegisterIkm = new(
            new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("<b>Регистрация</b>", DialogueTrigger.SignUp.ToString())
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
        MenuIkm = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Посмотреть информацию о тренере", DialogueTrigger.ConnectWithCoach.ToString())
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Редактировать личные данные", DialogueTrigger.EditPersonalInformation.ToString())
                }
            });
    }
}