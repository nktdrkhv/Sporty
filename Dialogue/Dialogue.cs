using System;
using System.Threading;
using System.Collections.Concurrent;
using Stateless;
using Stateless.Graph;

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
    private Message? _menuMessage;

    private bool _isInit = false;
    public async Task CheckInDb()
    {
        if (_isInit)
            await _machine.FoundInDb();
        else
        {
            _isInit = true;
            await _machine.NotFoundInDb();
        }
    }

    private Dialogue(ITelegramBotClient botClient, long id)
    {
        _machine = new DialogueMachine(this);
        _botClient = botClient;
        Id = id;
        Data = new();
    }

    private Dialogue(ITelegramBotClient botClient, Message lastRecievedMessage) : this(botClient, lastRecievedMessage.Chat.Id) => _lastRecievedMessage = lastRecievedMessage;

    // ---------------------- Main static outer handlers ----------------------

    public static async Task HandleTextInputAsync(ITelegramBotClient botClient, Message message)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(message.Chat.Id, new Dialogue(botClient, message));
        currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Console.WriteLine($"message -- {message.Text}");

        await currentDialogue._machine.ProcessTransition(message.Text ?? "Ошибка");
    }

    public static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(callbackQuery.From.Id, new Dialogue(botClient, callbackQuery.From.Id));
        currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Console.WriteLine($"callback -- {callbackQuery.Data!.ToDialogueTrigger()}");

        await currentDialogue._machine.ProcessTransition(callbackQuery.Data?.ToDialogueTrigger() ?? DialogueTrigger.BackToMenu);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    public static async Task HandleStartAsync(ITelegramBotClient botClient, Message message)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(message.Chat.Id, new Dialogue(botClient, message));
        currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await currentDialogue._machine.ActivateStateMachine();
    }

    // ---------------------- Callbacks for Dialogue machine ----------------------

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
        return await _botClient.SendTextMessageAsync(
            chatId: Id,
            text: DialogueHelper.RegisterText,
            replyMarkup: DialogueHelper.RegisterIkm,
            parseMode: ParseMode.Html);
    }

    public async Task<Message> SendRegistrationSequence(DialogueState state)
    {
        var text = state switch
        {
            DialogueState.NameInput => DialogueHelper.RegSeqNameText,
            DialogueState.GenderInput => DialogueHelper.RegSeqGenderText,
            DialogueState.AgeInput => DialogueHelper.RegSeqAgeText,
            DialogueState.HeightInput => DialogueHelper.RegSeqHeightText,
            DialogueState.WeightInput => DialogueHelper.RegSeqWeightText,
            DialogueState.EmailInput => DialogueHelper.RegSeqEmailText,
            _ => DialogueHelper.RegSeqUnknownText,
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

    public async Task<Message> SendCoachInfo()
    {
        return null;
    }

    public async Task<Message> SendPersonalInfo()
    {
        return null;
    }

    public async Task<Message> SendPersonalFieldEditor(DialogueState state)
    {
        return null;
    }
}