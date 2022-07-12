using System;
using System.Collections.Generic;
using System.Collections;

namespace Sporty.Dialogue;

public class DialoguePoll
{
    private DialogueState[] _stages;
    private int _index = 0;

    public bool IsEnd { get; private set; } = false;

    public DialoguePoll(params DialogueState[] stages) => _stages = stages;

    public DialogueState First() => _stages[0];

    public DialogueState Current() => _stages[_index];

    public DialogueState Next()
    {
        if (_index <= _stages.Length - 1)
            return _stages[_index++];
        else
        {
            IsEnd = true;
            return DialogueState.Null;
        }
    }

    public void Restart()
    {
        _index = 0;
        IsEnd = false;
    }
}