using Stateless;

using Sporty.Data;

namespace Sporty.Dialogue;

public class DialogueMachine
{
    private Dialogue _dialogue;
    private DialoguePoll _registrationPoll;
    private DialogueTrigger? _lastTrigger;

    private StateMachine<DialogueState, DialogueTrigger> _stateMachine;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<string> _setTextTrigger;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<DialogueTrigger> _setDoubleTrigger;

    public DialogueMachine(Dialogue dialogue)
    {
        _dialogue = dialogue;

        _stateMachine = new StateMachine<DialogueState, DialogueTrigger>(DialogueState.Initialization);
        _setTextTrigger = _stateMachine.SetTriggerParameters<string>(DialogueTrigger.TextInput);
        _setDoubleTrigger = _stateMachine.SetTriggerParameters<DialogueTrigger>(DialogueTrigger.TriggerInput);

        _registrationPoll = new DialoguePoll(
            DialogueTrigger.NameChange,
            DialogueTrigger.GenderChange,
            DialogueTrigger.AgeChange,
            DialogueTrigger.HeightChange,
            DialogueTrigger.WeightChange,
            DialogueTrigger.EmailChange
        );

        Configure();
    }

    // ---------------------- Configurations ----------------------

    private void Configure()
    {
        _stateMachine.OnUnhandledTriggerAsync(async (state, trigger) => await _dialogue.SendUnsupportedInputMessageAsync());

        _stateMachine.Configure(DialogueState.Initialization)
            .Permit(DialogueTrigger.IsInDb, DialogueState.Menu)
            .Permit(DialogueTrigger.IsNotInDb, DialogueState.Unregistered)
            .OnActivateAsync(async () => await _dialogue.CheckInDb());

        _stateMachine.Configure(DialogueState.Unregistered)
            .Permit(DialogueTrigger.SignUp, DialogueState.Registration)
            .OnEntryAsync(async () =>
            {
                await _dialogue.SendWelcomeMessageAsync();
                await _dialogue.SendRegisterWarningAsync();
            })
            .OnExitAsync(async () => await _dialogue.RegistrationApproveActions()); //

        _stateMachine.Configure(DialogueState.Registration)
            .OnEntryAsync(async () => await _dialogue.SendCustomerDataSequenceAsync(_registrationPoll.First()))
            .InternalTransitionAsync<string>(_setTextTrigger, async (text, _) =>
            {
                try
                {
                    switch (_registrationPoll.Current())
                    {
                        case DialogueTrigger.NameChange: _dialogue.Data.Customer.Name = PersonValidator.Name(text); break;
                        case DialogueTrigger.GenderChange: _dialogue.Data.Customer.Gender = PersonValidator.Gender(text); break;
                        case DialogueTrigger.AgeChange: _dialogue.Data.Customer.Age = PersonValidator.Age(text); break;
                        case DialogueTrigger.HeightChange: _dialogue.Data.Customer.Height = PersonValidator.Height(text); break;
                        case DialogueTrigger.WeightChange: _dialogue.Data.Customer.Weight = PersonValidator.Weight(text); break;
                        case DialogueTrigger.EmailChange: _dialogue.Data.Customer.Email = PersonValidator.Email(text); break;
                        default: return;
                    };
                }
                catch (ArgumentException e)
                {
                    await _dialogue.SendDataInputErrorAsync(e.Message);
                    return;
                }

                var nextState = _registrationPoll.Next();
                if (nextState is not DialogueTrigger.Null)
                    await _dialogue.SendCustomerDataSequenceAsync(nextState);
                else
                    await _stateMachine.FireAsync(DialogueTrigger.GoToMenu);
            })
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu);

        _stateMachine.Configure(DialogueState.Menu)
            .OnEntryAsync(async () => await _dialogue.SendMenuAsync())
            .Permit(DialogueTrigger.WatchPersonalInformation, DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.ConnectWithCoach, DialogueState.CoachInformationView);

        _stateMachine.Configure(DialogueState.PersonalInformationView)
            .OnEntryAsync(async () => await _dialogue.SendPersonalInformationAsync())
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.EditPersonalInformation, DialogueState.PersonalInformationChange);

        _stateMachine.Configure(DialogueState.PersonalInformationChange)
            .OnEntryAsync(async () => await _dialogue.SendPersonalFieldEditorAsync())
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.Redo, DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.WatchPersonalInformation, DialogueState.PersonalInformationView)
            .InternalTransitionAsync(_setDoubleTrigger, async (trigger, t) =>
            {
                _lastTrigger = trigger;
                await _dialogue.SendCustomerDataSequenceAsync(trigger);
            })
            .InternalTransitionAsync<string>(_setTextTrigger, async (text, t) =>
            {
                try
                {
                    switch (_lastTrigger)
                    {
                        case DialogueTrigger.NameChange: _dialogue.Data.Customer.Name = PersonValidator.Name(text); break;
                        case DialogueTrigger.GenderChange: _dialogue.Data.Customer.Gender = PersonValidator.Gender(text); break;
                        case DialogueTrigger.AgeChange: _dialogue.Data.Customer.Age = PersonValidator.Age(text); break;
                        case DialogueTrigger.HeightChange: _dialogue.Data.Customer.Height = PersonValidator.Height(text); break;
                        case DialogueTrigger.WeightChange: _dialogue.Data.Customer.Weight = PersonValidator.Weight(text); break;
                        case DialogueTrigger.EmailChange: _dialogue.Data.Customer.Email = PersonValidator.Email(text); break;
                        default: return;
                    };
                }
                catch (ArgumentException e)
                {
                    await _dialogue.SendDataInputErrorAsync(e.Message);
                }
                finally
                {
                    await _stateMachine.FireAsync(DialogueTrigger.WatchPersonalInformation);
                }
            });

        _stateMachine.Configure(DialogueState.CoachInformationView)
            .OnEntryAsync(async () => await _dialogue.SendCoachInformationAsync())
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu);
    }

    // ---------------------- Callbacks for dialog ----------------------

    public async Task ActivateStateMachine() => await _stateMachine.ActivateAsync();

    public async Task ProcessTransition(DialogueTrigger trigger) => await _stateMachine.FireAsync(trigger);

    public async Task ProcessDoubleTransition(DialogueTrigger trigger) => await _stateMachine.FireAsync(_setDoubleTrigger, trigger);

    public async Task ProcessTransition(string text) => await _stateMachine.FireAsync(_setTextTrigger, text);
}