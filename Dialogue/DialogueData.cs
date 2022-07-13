using Sporty.Data;

namespace Sporty.Dialogue;

public class DialogueData
{
    public Person Customer { get; set; }

    public DialogueData() => Customer = new();
}