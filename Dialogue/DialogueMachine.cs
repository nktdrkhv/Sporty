using System;
using Stateless;

namespace Sporty.Dialogue;

public class DialogueMachine
{
    private Dialogue _dialogue;

    private StateMachine<DialogueState, DialogueTrigger> _stateMachine;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<string> _setPersonDataTrigger;

    private DialoguePoll _registrationPoll;

    public DialogueMachine(Dialogue dialogue)
    {
        _dialogue = dialogue;

        _stateMachine = new StateMachine<DialogueState, DialogueTrigger>(DialogueState.Initialization);
        _setPersonDataTrigger = _stateMachine.SetTriggerParameters<string>(DialogueTrigger.PersonDataInput);

        _registrationPoll = new DialoguePoll(
            DialogueState.NameInput,
            DialogueState.GenderInput,
            DialogueState.AgeInput,
            DialogueState.HeightInput,
            DialogueState.WeightInput,
            DialogueState.EmailInput
        );
        Configure();
    }

    // ---------------------- Configurations ----------------------

    public void Configure()
    {
        _stateMachine.Configure(DialogueState.Initialization)
            .Permit(DialogueTrigger.IsInDb, DialogueState.Menu)
            .Permit(DialogueTrigger.IsNotInDb, DialogueState.Unregistered)
            .OnActivateAsync(async () => await _dialogue.CheckInDb());

        _stateMachine.Configure(DialogueState.Unregistered)
            .Permit(DialogueTrigger.SignUp, DialogueState.Registration)
            .OnEntryAsync(async () =>
            {
                await _dialogue.SendWelcomeMessage();
                await _dialogue.SendRegisterWarningMessage();
            });

        ConfigureRegisterSteps();

        _stateMachine.Configure(DialogueState.Menu)
            .Permit(DialogueTrigger.WatchPersonalInformation, DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.ConnectWithCoach, DialogueState.CoachInfoView);

        _stateMachine.Configure(DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.BackToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.EditPersonalInformation, DialogueState.PersonalInformationChange);

        _stateMachine.Configure(DialogueState.PersonalInformationChange)
            .Permit(DialogueTrigger.BackToMenu, DialogueState.Menu);
    }

    public void ConfigureRegisterSteps()
    {
        _stateMachine.Configure(DialogueState.Registration)
            .InternalTransitionAsync<string>(DialogueTrigger.PersonDataInput, async (text) =>
            {
                var nextState = _registrationPoll.Next();
                if (nextState is not DialogueState.Null)
                    await _dialogue.SendRegistrationSequence(nextState);
                else
                    await _stateMachine.FireAsync(DialogueTrigger.BackToMenu);
            })
            .Permit(DialogueTrigger.BackToMenu, DialogueState.Menu);


        // _stateMachine.Configure(DialogueState.Registration)
        //     .InitialTransition(DialogueState.NameInput);

        // _stateMachine.Configure(DialogueState.NameInput)
        //     .SubstateOf(DialogueState.Registration)
        //     //.Permit(DialogueTrigger.PersonDataInput, DialogueState.GenderInput)
        //     .OnEntryAsync(async (transition) => await _dialogue.SendRegistrationSequence(transition.Destination))
        //     .OnEntryFromAsync<string>(_setPersonDataTrigger, async (text) =>
        //     {
        //         Console.WriteLine($"Обработан ввод:{text}");
        //     });

        // _stateMachine.Configure(DialogueState.GenderInput)
        //     .SubstateOf(DialogueState.Registration)
        //     .Permit(DialogueTrigger.MaleInput, DialogueState.AgeInput)
        //     .Permit(DialogueTrigger.FemaleInput, DialogueState.AgeInput)
        //     .OnEntryAsync((transition) => _dialogue.SendRegistrationSequence(transition.Destination));

        // _stateMachine.Configure(DialogueState.AgeInput)
        //     .SubstateOf(DialogueState.Registration)
        //     .Permit(DialogueTrigger.PersonDataInput, DialogueState.HeightInput)
        //     .OnEntryAsync((transition) => _dialogue.SendRegistrationSequence(transition.Destination))
        //     .OnEntryFromAsync<string>(_setPersonDataTrigger, async (text) =>
        //     {
        //         Console.WriteLine($"Обработан ввод:{text}");
        //     });

        // _stateMachine.Configure(DialogueState.HeightInput)
        //     .SubstateOf(DialogueState.Registration)
        //     .Permit(DialogueTrigger.PersonDataInput, DialogueState.WeightInput)
        //     .OnEntryAsync((transition) => _dialogue.SendRegistrationSequence(transition.Destination))
        //     .OnEntryFromAsync<string>(_setPersonDataTrigger, async (text) =>
        //     {
        //         Console.WriteLine($"Обработан ввод:{text}");
        //     });

        // _stateMachine.Configure(DialogueState.WeightInput)
        //     .SubstateOf(DialogueState.Registration)
        //     .Permit(DialogueTrigger.PersonDataInput, DialogueState.EmailInput)
        //     .OnEntryAsync((transition) => _dialogue.SendRegistrationSequence(transition.Destination))
        //     .OnEntryFromAsync<string>(_setPersonDataTrigger, async (text) =>
        //     {
        //         Console.WriteLine($"Обработан ввод:{text}");
        //     });

        // _stateMachine.Configure(DialogueState.EmailInput)
        //     .SubstateOf(DialogueState.Registration)
        //     .Permit(DialogueTrigger.PersonDataInput, DialogueState.Menu)
        //     .OnEntryAsync((transition) => _dialogue.SendRegistrationSequence(transition.Destination))
        //     .OnEntryFromAsync<string>(_setPersonDataTrigger, async (text) =>
        //     {
        //         Console.WriteLine($"Обработан ввод:{text}");
        //     });
    }

    // ---------------------- Callbacks for dialog ----------------------

    public async Task ActivateStateMachine() => await _stateMachine.ActivateAsync();

    public async Task FoundInDb() => await _stateMachine.FireAsync(DialogueTrigger.IsInDb);

    public async Task NotFoundInDb() => await _stateMachine.FireAsync(DialogueTrigger.IsNotInDb);

    public async Task ProcessTransition(DialogueTrigger trigger) => await _stateMachine.FireAsync(trigger);

    public async Task ProcessTransition(string text) => await _stateMachine.FireAsync(_setPersonDataTrigger, text);
}