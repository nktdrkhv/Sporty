namespace Sporty.Dialogue;

public enum DialogueTrigger
{
    SignUp, IsInDb, IsNotInDb,
    TextInput, TriggerInput,
    ProgramPurchase, ConfirmPurchase, SuccessPurchase,
    NameChange, GenderChange, AgeChange, HeightChange, WeightChange, EmailChange,
    WatchPersonalInformation, EditPersonalInformation, ConnectWithCoach,
    Redo, GoToMenu, Null
}

public enum DialogueState
{
    Initialization, Unregistered, Registration,
    PersonalInformationView, PersonalInformationChange, CoachInformationView,
    PurchaseDecision,
    Menu, Null
}