using System;
using System.Threading;
using System.Collections.Concurrent;
using Stateless;
using Stateless.Graph;
using Sporty.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sporty.Dialogue;

public class Dialogue
{
    public static ConcurrentDictionary<long, Dialogue> RecentUsers = new();

    public Person? Customer { get; private set; }

    public long LastActionTime { get; private set; }

    private DialogueMachine _machine;

    private Message? _lastRecievedMessage;

    private Message? _lastSentMessage;

    private Message? _menuMessage;

    private Dialogue()
    {
        _machine = DialogueMachine.Create();
    }

    private Dialogue(Message lastRecievedMessage) : base()
    {
        _lastRecievedMessage = lastRecievedMessage;
    }

    public static async Task<Message> HandleTextInputAsync(Message message)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(message.Chat.Id, new Dialogue(message));
        currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


        return null;
    }

    public static async Task<Message> HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(callbackQuery.From.Id, new Dialogue());
        currentDialogue.LastActionTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return null;
    }

    //private void async ProcessTextViaMachineAsync(Message message)
    //{}
}