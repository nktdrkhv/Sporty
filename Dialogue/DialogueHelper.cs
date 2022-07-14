using Telegram.Bot.Types.ReplyMarkups;

using Sporty.Data;

namespace Sporty.Dialogue;

public static class DialogueHelper
{
    public static DialogueTrigger ToDialogueTrigger(this string str) => Enum.Parse<DialogueTrigger>(str);
    public static DialogueState ToDialogueState(this string str) => Enum.Parse<DialogueState>(str);

    // --------------------------------------------------------------------------------

    public static string UnsupportedText = "<b>Неожиданно!</b>";
    public static string WelcomeText = "<i>Приветсвенное сообщение</i>";
    public static string RegisterText = "Для начала вам необходимо <b>зарегистрироваться</b>";
    public static string SeqDataErrorText = "Введены некорректные данные";
    public static string SeqNameText = "Укажите имя";
    public static string SeqGenderText = "Укажите пол";
    public static string SeqAgeText = "Укажите возраст";
    public static string SeqHeightText = "Укажите рост в сантиметрах";
    public static string SeqWeightText = "Укажите вес в килограмммах";
    public static string SeqEmailText = "Укажите электронную почту";
    public static string SeqUnknownText = "Произошла ошибка";
    public static string MenuText(string name) => $"Здравствуйте, <b>{name}</b>!";
    public static string CoachText => "Информация о <i>тренере</i>";
    public static string PersonalText(Person p) =>
        "<b>Персональная информация</b>\n" +
        $"\n<i>Имя:</i> {p.Name}" +
        $"\n<i>Пол:</i> {p.Gender}" +
        $"\n<i>Возраст:</i> {p.Age}" +
        $"\n<i>Рост:</i> {p.Height}" +
        $"\n<i>Вес:</i> {p.Weight}" +
        $"\n<i>Почта:</i> {p.Email}";

    // --------------------------------------------------------------------------------

    public static InlineKeyboardMarkup RegisterIkm;
    public static InlineKeyboardMarkup GenderIkm;
    public static InlineKeyboardMarkup MenuIkm;
    public static InlineKeyboardMarkup CoachIkm;
    public static InlineKeyboardMarkup PersonalInformationIkm;
    public static InlineKeyboardMarkup PersonalFieldsIkm;
    public static InlineKeyboardMarkup RedoOrMenuIkm;
    public static InlineKeyboardMarkup RedoIkm;

    public static InlineKeyboardButton MenuButton;
    public static InlineKeyboardButton RedoButton;

    // --------------------------------------------------------------------------------

    static DialogueHelper()
    {
        MenuButton = InlineKeyboardButton.WithCallbackData("В меню", DialogueTrigger.GoToMenu.ToString());
        RedoButton = InlineKeyboardButton.WithCallbackData("Отменить", DialogueTrigger.Redo.ToString());

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
                    InlineKeyboardButton.WithCallbackData("Муж.", "= Мужчина"),
                    InlineKeyboardButton.WithCallbackData("Жен.", "= Женщина")
                }
            });
        MenuIkm = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Информация о тренере", DialogueTrigger.ConnectWithCoach.ToString())
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Личные данные", DialogueTrigger.WatchPersonalInformation.ToString())
                }
            });
        CoachIkm = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("Написать тренеру", @"https://t.me/nktdrkhv"),
                    MenuButton
                },
            });
        PersonalInformationIkm = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Редактировать", DialogueTrigger.EditPersonalInformation.ToString()),
                    MenuButton,
                }
            });

        PersonalFieldsIkm = new(
            new[]
            {
                new[]
                {
                    RedoButton,
                    MenuButton,
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Имя", "* " + DialogueTrigger.NameChange.ToString()),
                    InlineKeyboardButton.WithCallbackData("Пол", "* " + DialogueTrigger.GenderChange.ToString()),
                    InlineKeyboardButton.WithCallbackData("Возраст", "* " + DialogueTrigger.AgeChange.ToString()),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Рост", "* " + DialogueTrigger.HeightChange.ToString()),
                    InlineKeyboardButton.WithCallbackData("Вес", "* " + DialogueTrigger.WeightChange.ToString()),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Электронная почта", "* " + DialogueTrigger.EmailChange.ToString()),
                }
            });
        RedoOrMenuIkm = new(
            new[]
            {
                new[]
                {
                    RedoButton,
                    MenuButton,
                }
            });
        RedoIkm = new(
            new[]
            {
                new[]
                {
                    RedoButton,
                }
            });
    }
}