namespace Sporty.Dialogue;

public class DialoguePoll
{
    private DialogueTrigger[] _steps;
    private int _index = 0;

    public DialoguePoll(params DialogueTrigger[] steps) => _steps = steps;

    public DialogueTrigger First() => _steps[0];
    public DialogueTrigger Current() => _steps[_index];
    public bool IsEnd { get; private set; } = false;

    public DialogueTrigger Next()
    {
        _index += 1;
        if (_index <= _steps.Length - 1)
            return _steps[_index];
        else
        {
            IsEnd = true;
            return DialogueTrigger.Null;
        }
    }

    public void Restart()
    {
        _index = 0;
        IsEnd = false;
    }
}