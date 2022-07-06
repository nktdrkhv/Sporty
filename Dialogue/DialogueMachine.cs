using System;
using Stateless;

namespace Sporty.Dialogue;

public class DialogueMachine
{
    private StateMachine<DialogueState, DialogueTrigger> _stateMachine;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<string> _setPersonDataTrigger;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<bool> _setPersonDataBoolTrigger;

    private DialogueMachine(StateMachine<DialogueState, DialogueTrigger> stateMachine)
    {
        _stateMachine = stateMachine;
        _setPersonDataTrigger = _stateMachine.SetTriggerParameters<string>(DialogueTrigger.PersonDataInput);
        _setPersonDataBoolTrigger = _stateMachine.SetTriggerParameters<bool>(DialogueTrigger.PersonDataInput);
    }

    public static DialogueMachine Create()
    {
        var stateMachine = new StateMachine<DialogueState, DialogueTrigger>(DialogueState.Initialization);

        stateMachine.Configure(DialogueState.Initialization)
            .Permit(DialogueTrigger.IsInDb, DialogueState.Menu)
            .Permit(DialogueTrigger.IsNotInDb, DialogueState.Unregistered);

        stateMachine.Configure(DialogueState.Unregistered)
            .Permit(DialogueTrigger.SignUp, DialogueState.Registration);

        stateMachine.Configure(DialogueState.NameInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.GenderInput);

        stateMachine.Configure(DialogueState.GenderInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.AgeInput);

        stateMachine.Configure(DialogueState.AgeInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.HeightInput);

        stateMachine.Configure(DialogueState.HeightInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.WeightInput);

        stateMachine.Configure(DialogueState.WeightInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.EmailInput);

        stateMachine.Configure(DialogueState.EmailInput)
            .SubstateOf(DialogueState.Registration)
            .Permit(DialogueTrigger.PersonDataInput, DialogueState.Menu);

        stateMachine.Configure(DialogueState.Menu)
            .Permit(DialogueTrigger.WatchPersonalInformation, DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.ConnectWithCoach, DialogueState.CoachInfoView);

        stateMachine.Configure(DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.BackToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.EditPersonalInformation, DialogueState.PersonalInformationChange);

        stateMachine.Configure(DialogueState.PersonalInformationChange)
            .Permit(DialogueTrigger.BackToMenu, DialogueState.Menu);

        return new DialogueMachine(stateMachine);
    }

    public async Task ProcessTransition(DialogueTrigger trigger)
    {
        await _stateMachine.FireAsync(trigger);
    }

    public async Task ProcessTransition(string text)
    {
        await _stateMachine.FireAsync(_setPersonDataTrigger, text);
    }

}

public enum DialogueTrigger
{
    IsInDb, IsNotInDb,
    SignUp, PersonDataInput,
    WatchPersonalInformation, EditPersonalInformation, ConnectWithCoach,
    NameChange, GenderChange, AgeChange, HeightChange, WeightChange, EmailChange,
    BackToMenu
}

public enum DialogueState
{
    Initialization, Unregistered, Registration,
    NameInput, GenderInput, AgeInput, HeightInput, WeightInput, EmailInput,
    PersonalInformationView, PersonalInformationChange, CoachInfoView,
    Menu
}