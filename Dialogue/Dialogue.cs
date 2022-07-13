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
    public long LastActionTime { get; private set; }
    public long Id { get; private set; }

    private DialogueMachine _machine;
    private ITelegramBotClient _botClient;

    private Message? _lastRecievedMessage;
    private Message? _lastSentMessage;
    private Message? _systemMessage;

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
        Console.WriteLine("new dialogue");
    }

    private Dialogue(ITelegramBotClient botClient, Message lastRecievedMessage) : this(botClient, lastRecievedMessage.Chat.Id) => _lastRecievedMessage = lastRecievedMessage;

    // ---------------------- Main static outer handlers ----------------------

    public static async Task HandleTextInputAsync(ITelegramBotClient botClient, Message message)
    {
        if (RecentUsers.TryGetValue(message.Chat.Id, out var currentDialogue))
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        else
        {
            currentDialogue = new Dialogue(botClient, message.Chat.Id);
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            RecentUsers.TryAdd(message.Chat.Id, currentDialogue);
        }

        await currentDialogue._machine.ProcessTransition(message.Text ?? DialogueHelper.SeqUnknownText);
    }

    public static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        if (RecentUsers.TryGetValue(callbackQuery.From.Id, out var currentDialogue))
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        else
        {
            currentDialogue = new Dialogue(botClient, callbackQuery.From.Id);
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            RecentUsers.TryAdd(callbackQuery.From.Id, currentDialogue);
        }

        await currentDialogue._machine.ProcessTransition(callbackQuery.Data?.ToDialogueTrigger() ?? DialogueTrigger.GoToMenu);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    public static async Task HandleStartAsync(ITelegramBotClient botClient, Message message)
    {
        if (RecentUsers.TryGetValue(message.Chat.Id, out var currentDialogue))
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        else
        {
            currentDialogue = new Dialogue(botClient, message.Chat.Id);
            currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            RecentUsers.TryAdd(message.Chat.Id, currentDialogue);
        }
    }

    // ---------------------- Callbacks for Dialogue machine ----------------------
    public async Task<Message> ClearSystemMessageReplyMarkup() => await ReplaceSystemMessageReplyMarkup(null);

    public async Task<Message> ReplaceSystemMessageReplyMarkup(InlineKeyboardMarkup? ikm) => await _botClient.EditMessageReplyMarkupAsync(_systemMessage!.Chat.Id, _systemMessage.MessageId, ikm);

    public async Task<Message> SendWelcomeMessage()
    {
        await _botClient.SendChatActionAsync(Id, ChatAction.Typing);
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.WelcomeText,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendRegisterWarningMessage()
    {
        await _botClient.SendChatActionAsync(Id, ChatAction.Typing);
        var msg = await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.RegisterText,
            replyMarkup: DialogueHelper.RegisterIkm,
            parseMode: ParseMode.Html);
        _systemMessage = msg;
        return msg;
    }

    public async Task<Message> SendDataInputErrorMessage(string? text = null)
    {
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: text ?? DialogueHelper.SeqDataErrorText,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendCustomerDataSequence(DialogueState state)
    {
        var text = state switch
        {
            DialogueState.NameInput => DialogueHelper.SeqNameText,
            DialogueState.GenderInput => DialogueHelper.SeqGenderText,
            DialogueState.AgeInput => DialogueHelper.SeqAgeText,
            DialogueState.HeightInput => DialogueHelper.SeqHeightText,
            DialogueState.WeightInput => DialogueHelper.SeqWeightText,
            DialogueState.EmailInput => DialogueHelper.SeqEmailText,
            _ => DialogueHelper.SeqUnknownText,
        };

        switch (state)
        {
            case DialogueState.GenderInput:
                return await _botClient.SendTextMessageAsync(
                chatId: Id,
                text: text,
                replyMarkup: DialogueHelper.GenderIkm);
            default:
                return await _botClient.SendTextMessageAsync(
                chatId: Id,
                text: text,
                parseMode: ParseMode.Html);
        }
    }

    public async Task<Message> SendMenu()
    {
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.MenuText(Data.Customer.Name!),
            replyMarkup: DialogueHelper.MenuIkm,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendCoachInformation()
    {
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.CoachText,
            replyMarkup: DialogueHelper.CoachIkm,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendPersonalInformation()
    {
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.PersonalText(Data.Customer),
            replyMarkup: DialogueHelper.PersonalInformationIkm,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendPersonalFieldEditor()
    {
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.PersonalText(Data.Customer),
            replyMarkup: DialogueHelper.PersonalFieldsIkm,
            parseMode: ParseMode.Html);
    }

    // public async Task<Message> SendPersonalFieldInput(DialogueState state)
    // {
    //     return null;
    // }
}