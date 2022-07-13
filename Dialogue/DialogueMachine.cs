using FluentValidation;
using Stateless;

using Sporty.Data;

namespace Sporty.Dialogue;

public class DialogueMachine
{
    private Dialogue _dialogue;

    private StateMachine<DialogueState, DialogueTrigger> _stateMachine;
    private StateMachine<DialogueState, DialogueTrigger>.TriggerWithParameters<string> _setTextTrigger;

    private DialoguePoll _registrationPoll;
    private PersonValidator _validator = new();

    public DialogueMachine(Dialogue dialogue)
    {
        _dialogue = dialogue;

        _stateMachine = new StateMachine<DialogueState, DialogueTrigger>(DialogueState.Initialization);
        _setTextTrigger = _stateMachine.SetTriggerParameters<string>(DialogueTrigger.TextInput);

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
            })
            .OnExitAsync(async () => await _dialogue.ClearSystemMessageReplyMarkup());

        _stateMachine.Configure(DialogueState.Registration)
            .OnEntryAsync(async () => await _dialogue.SendCustomerDataSequence(_registrationPoll.First()))
            .InternalTransitionAsync<string>(_setTextTrigger, async (text, t) =>
            {
                try
                {
                    //var person = _dialogue.Data.Customer;
                    switch (_registrationPoll.Current())
                    {//validate and theow don't take more then one args
                        case DialogueState.NameInput:
                            _dialogue.Data.Customer.Name = text;
                            _validator.Validate(_dialogue.Data.Customer, x => x.IncludeProperties(x => x.Name));
                            break;
                        case DialogueState.AgeInput:
                            _dialogue.Data.Customer.Age = int.Parse(text);
                            _validator.Validate(_dialogue.Data.Customer, x => x.IncludeProperties(x => x.Age));
                            break;
                        case DialogueState.HeightInput:
                            _dialogue.Data.Customer.Height = int.Parse(text);
                            _validator.Validate(_dialogue.Data.Customer, x => x.IncludeProperties(x => x.Height));
                            break;
                        case DialogueState.WeightInput:
                            _dialogue.Data.Customer.Weight = int.Parse(text);
                            _validator.Validate(_dialogue.Data.Customer, x => x.IncludeProperties(x => x.Weight));
                            break;
                        case DialogueState.EmailInput:
                            _dialogue.Data.Customer.Email = text;
                            _validator.Validate(_dialogue.Data.Customer, x => x.IncludeProperties(x => x.Email));
                            break;
                        default:
                            return;
                    };
                }
                catch (ValidationException e)
                {
                    await _dialogue.SendDataInputErrorMessage(e.Message);
                    await _dialogue.SendCustomerDataSequence(_registrationPoll.Current());
                    return;
                }
                catch
                {
                    await _dialogue.SendDataInputErrorMessage();
                    await _dialogue.SendCustomerDataSequence(_registrationPoll.Current());
                    return;
                }

                var nextState = _registrationPoll.Next();
                if (nextState is not DialogueState.Null)
                    await _dialogue.SendCustomerDataSequence(nextState);
                else
                    await _stateMachine.FireAsync(DialogueTrigger.GoToMenu);
            })
            .InternalTransitionAsync(DialogueTrigger.MaleInput, async () =>
            {
                _dialogue.Data.Customer.Gender = "Мужской";

                var nextState = _registrationPoll.Next();
                if (nextState is not DialogueState.Null)
                    await _dialogue.SendCustomerDataSequence(nextState);
                else
                    await _stateMachine.FireAsync(DialogueTrigger.GoToMenu);
            })
            .InternalTransitionAsync(DialogueTrigger.FemaleInput, async () =>
            {
                _dialogue.Data.Customer.Gender = "Женский";

                var nextState = _registrationPoll.Next();
                if (nextState is not DialogueState.Null)
                    await _dialogue.SendCustomerDataSequence(nextState);
                else
                    await _stateMachine.FireAsync(DialogueTrigger.GoToMenu);
            })

            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu);

        _stateMachine.Configure(DialogueState.Menu)
            .OnEntryAsync(async () => await _dialogue.SendMenu())
            .Permit(DialogueTrigger.WatchPersonalInformation, DialogueState.PersonalInformationView)
            .Permit(DialogueTrigger.ConnectWithCoach, DialogueState.CoachInformationView);

        _stateMachine.Configure(DialogueState.PersonalInformationView)
            .OnEntryAsync(async () => await _dialogue.SendPersonalInformation())
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.EditPersonalInformation, DialogueState.PersonalInformationChange);

        _stateMachine.Configure(DialogueState.PersonalInformationChange)
            .OnEntryAsync(async () => await _dialogue.SendPersonalFieldEditor())
            //.OnExit
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu)
            .Permit(DialogueTrigger.Redo, DialogueState.PersonalInformationView);

        _stateMachine.Configure(DialogueState.CoachInformationView)
            .OnEntryAsync(async () => await _dialogue.SendCoachInformation())
            .Permit(DialogueTrigger.GoToMenu, DialogueState.Menu);
    }

    // ---------------------- Callbacks for dialog ----------------------

    public async Task ActivateStateMachine() => await _stateMachine.ActivateAsync();

    public async Task ProcessTransition(DialogueTrigger trigger) => await _stateMachine.FireAsync(trigger);

    public async Task ProcessTransition(string text) => await _stateMachine.FireAsync(_setTextTrigger, text);
}