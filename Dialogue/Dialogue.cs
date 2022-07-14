using System.Collections.Concurrent;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sporty.Dialogue;

public class Dialogue
{
    public static ConcurrentDictionary<long, Dialogue> RecentUsers = new();

    public DialogueData Data { get; private set; }
    public long Id { get; private set; }
    public bool IgnoreRequest { get; set; }
    public long LastActionTime
    {
        get { return _lastActionTime; }
        set
        {
            IgnoreRequest = value - _lastActionTime < 100L;
            _lastActionTime = value;
        }
    }
    private long _lastActionTime = 0;

    private DialogueMachine _machine;
    private ITelegramBotClient _botClient;

    private CallbackQuery? _lastRecievedCallbackQuery;
    private Message? _lastRecievedMessage;

    private Message? _lastSentMessage;
    private Message? _dataInputMessage;
    private Message? _systemMessage;

    private bool ShouldResend
    {
        get
        {
            bool resend = true;
            if (_systemMessage is not null)
            {
                resend = _lastRecievedMessage?.Date.CompareTo(_systemMessage.Date) >= 0;
                resend = _lastSentMessage?.Date.CompareTo(_systemMessage.Date) >= 0;
            }
            return resend;
        }
    }

    private bool _isInit = false;
    public async Task CheckInDb()
    {
        if (_isInit)
            await _machine.ProcessTransition(DialogueTrigger.IsInDb);
        else
        {
            _isInit = true;
            await _machine.ProcessTransition(DialogueTrigger.IsNotInDb);
        }
    }

    private Dialogue(ITelegramBotClient botClient, long id)
    {
        _machine = new DialogueMachine(this);
        _botClient = botClient;
        Id = id;
        Data = new();
        _machine.ActivateStateMachine().Wait();

        Console.WriteLine($"New dialogue with {Id}");
    }

    #region STATIC OUTER HANDLERS

    private static Dialogue? GetDialogue(ITelegramBotClient botCLient, long id)
    {
        if (RecentUsers.TryGetValue(id, out var currentDialogue))
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        else
        {
            currentDialogue = new Dialogue(botCLient, id);
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            RecentUsers.TryAdd(id, currentDialogue);
        }

        if (currentDialogue.IgnoreRequest)
            return null;
        else
            return currentDialogue;
    }

    public static async Task HandleTextAsync(ITelegramBotClient botClient, Message message)
    {
        var currentDialogue = GetDialogue(botClient, message.From!.Id);
        if (currentDialogue is null)
            return;
        currentDialogue._lastRecievedMessage = message;
        await currentDialogue._machine.ProcessTransition(message.Text ?? DialogueHelper.SeqUnknownText);
    }

    public static async Task HandleTextAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var currentDialogue = GetDialogue(botClient, callbackQuery.From.Id);
        if (currentDialogue is null)
            return;
        currentDialogue._lastRecievedCallbackQuery = callbackQuery;
        await currentDialogue._machine.ProcessTransition(callbackQuery.Data ?? DialogueHelper.SeqUnknownText);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    public static async Task HandleTriggerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var currentDialogue = GetDialogue(botClient, callbackQuery.From.Id);
        if (currentDialogue is null)
            return;
        currentDialogue._lastRecievedCallbackQuery = callbackQuery;
        await currentDialogue._machine.ProcessTransition(callbackQuery.Data?.ToDialogueTrigger() ?? DialogueTrigger.GoToMenu);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    public static async Task HandleDoubleTriggerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var currentDialogue = GetDialogue(botClient, callbackQuery.From.Id);
        if (currentDialogue is null)
            return;
        currentDialogue._lastRecievedCallbackQuery = callbackQuery;
        await currentDialogue._machine.ProcessDoubleTransition(callbackQuery.Data?.ToDialogueTrigger() ?? DialogueTrigger.GoToMenu);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    public static async Task HandleStartCommandAsync(ITelegramBotClient botClient, Message message)
    {
        var currentDialogue = GetDialogue(botClient, message.From!.Id);
        if (currentDialogue is null)
            return;
        currentDialogue._lastRecievedMessage = message;
        await Task.Delay(1000);
    }

    public static async Task HandleMenuCommandAsync(ITelegramBotClient botClient, Message message) => await Task.Delay(1000);

    #endregion

    #region TELEGRAM SPECIAL

    public async Task<Message> SendMessageAsync(long id, string text, InlineKeyboardMarkup? ikm = null)
    {
        await _botClient.SendChatActionAsync(Id, ChatAction.Typing);
        return await _botClient.SendTextMessageAsync(
            chatId: id,
            text: text,
            replyMarkup: ikm,
            parseMode: ParseMode.Html
        );
    }

    public async Task<Message> EditMessageAsync(Message oldMsg, string newText, InlineKeyboardMarkup? newIkm = null) =>
        await _botClient.EditMessageTextAsync(
            chatId: oldMsg.Chat.Id,
            messageId: oldMsg.MessageId,
            text: newText,
            replyMarkup: newIkm,
            parseMode: ParseMode.Html);

    public async Task ReplaceMarkupAsync(Message? msg, InlineKeyboardMarkup? newIkm = null)
    {
        if (msg is not null)
            await _botClient.EditMessageReplyMarkupAsync(msg.Chat.Id, msg.MessageId, newIkm);
    }

    public void DeleteMessage(ref Message? msg)
    {
        try
        {
            if (msg is not null)
            {
                _botClient.DeleteMessageAsync(msg.Chat, msg.MessageId).Wait();
                msg = null;
            }
        }
        catch
        {
            Console.WriteLine("Ошибка при удалении сообщения");
        }
    }

    #endregion

    #region CALLBACKS FOR DIALOGUE MACHINE

    public async Task RegistrationApproveActions() => await ReplaceMarkupAsync(_lastSentMessage);

    public async Task SendUnsupportedInputMessageAsync()
    {
        var message = await SendMessageAsync(Id, DialogueHelper.UnsupportedText);
        await Task.Delay(1000);
        DeleteMessage(ref _lastRecievedMessage);
        DeleteMessage(ref message);
    }

    public async Task<Message> SendWelcomeMessageAsync()
    {
        _lastSentMessage = await SendMessageAsync(Id, DialogueHelper.WelcomeText);
        return _lastSentMessage;
    }

    public async Task<Message> SendRegisterWarningAsync()
    {
        _lastSentMessage = await SendMessageAsync(Id, DialogueHelper.RegisterText, DialogueHelper.RegisterIkm);
        return _lastSentMessage;
    }

    public async Task<Message> SendDataInputErrorAsync(string? text = null)
    {
        DeleteMessage(ref _dataInputMessage);
        DeleteMessage(ref _lastRecievedMessage);

        _dataInputMessage = await SendMessageAsync(Id, text ?? DialogueHelper.SeqDataErrorText);
        return _dataInputMessage;
    }

    public async Task<Message> SendCustomerDataSequenceAsync(DialogueTrigger state)
    {
        var text = state switch
        {
            DialogueTrigger.NameChange => DialogueHelper.SeqNameText,
            DialogueTrigger.GenderChange => DialogueHelper.SeqGenderText,
            DialogueTrigger.AgeChange => DialogueHelper.SeqAgeText,
            DialogueTrigger.HeightChange => DialogueHelper.SeqHeightText,
            DialogueTrigger.WeightChange => DialogueHelper.SeqWeightText,
            DialogueTrigger.EmailChange => DialogueHelper.SeqEmailText,
            _ => DialogueHelper.SeqUnknownText,
        };

        DeleteMessage(ref _dataInputMessage);

        switch (state)
        {
            case DialogueTrigger.GenderChange:
                _dataInputMessage = await SendMessageAsync(Id, text, DialogueHelper.GenderIkm);
                break;
            default:
                _dataInputMessage = await SendMessageAsync(Id, text);
                break;
        }
        return _dataInputMessage;
    }

    public async Task<Message> SendMenuAsync()
    {
        DeleteMessage(ref _dataInputMessage);

        if (ShouldResend)
            _systemMessage = await SendMessageAsync(Id, DialogueHelper.MenuText(Data.Customer.Name!), DialogueHelper.MenuIkm);
        else
            await EditMessageAsync(_systemMessage!, DialogueHelper.MenuText(Data.Customer.Name!), DialogueHelper.MenuIkm);
        return _systemMessage!;
    }

    public async Task<Message> SendCoachInformationAsync()
    {
        if (ShouldResend)
            _systemMessage = await SendMessageAsync(Id, DialogueHelper.CoachText, DialogueHelper.CoachIkm);
        else
            await EditMessageAsync(_systemMessage!, DialogueHelper.CoachText, DialogueHelper.CoachIkm);
        return _systemMessage!;
    }

    public async Task<Message> SendPersonalInformationAsync()
    {
        DeleteMessage(ref _dataInputMessage);
        if (ShouldResend)
            _systemMessage = await SendMessageAsync(Id, DialogueHelper.PersonalText(Data.Customer), DialogueHelper.PersonalInformationIkm);
        else
            await EditMessageAsync(_systemMessage!, DialogueHelper.PersonalText(Data.Customer), DialogueHelper.PersonalInformationIkm);
        return _systemMessage!;
    }

    public async Task<Message> SendPersonalFieldEditorAsync()
    {
        DeleteMessage(ref _dataInputMessage);
        if (ShouldResend)
            _systemMessage = await SendMessageAsync(Id, DialogueHelper.PersonalText(Data.Customer), DialogueHelper.PersonalFieldsIkm);
        else
            await ReplaceMarkupAsync(_systemMessage!, DialogueHelper.PersonalFieldsIkm);
        return _systemMessage!;
    }

    #endregion
}