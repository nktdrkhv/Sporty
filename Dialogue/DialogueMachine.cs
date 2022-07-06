using System;
using Stateless;

namespace Sporty.Dialogue;

public class DialogueMachine
{
    private static StateMachine<DialogueState, DialogueTrigger> StateMachine;

    static DialogueMachine()
    {
        StateMachine = new(DialogueState.Initialization);

        StateMachine.Configure(DialogueState.Initialization)
            .Permit(DialogueTrigger.IsInDb, DialogueState.Menu)
            .Permit(DialogueTrigger.IsNotInDb, DialogueState.Registration);


        //StateMachine.Configure(D)
    }


}

public enum DialogueTrigger
{
    IsInDb, IsNotInDb,
    SetName, SetGender, SetAge, SetWeight, SetEmail
}

public enum DialogueState { Initialization, Unregistered, Registration, Registered, Menu, PersonalInformationView, PersonalInformationChange, }