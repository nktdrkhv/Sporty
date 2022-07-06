using System;
using System.Threading;
using System.Collections.Concurrent;
using Stateless;
using Stateless.Graph;
using Sporty.Models;

namespace Sporty.Dialogue;

public class Dialogue
{
    public static ConcurrentDictionary<long, Dialogue> RecentUsers = new();

    private DialogueMachine _machine;

    private Person _customer;

    private long _lastMessage = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static async Task HandleTextInputAsync(long id, string? text)
    {
        Dialogue currentDialogue = RecentUsers.GetOrAdd(id, new Dialogue());
    }
}