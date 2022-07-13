namespace Sporty.Dialogue;

public enum DialogueTrigger
{
    SignUp, IsInDb, IsNotInDb,
    TextInput, MaleInput, FemaleInput,
    WatchPersonalInformation, EditPersonalInformation, ConnectWithCoach,
    NameChange, GenderChange, AgeChange, HeightChange, WeightChange, EmailChange,
    Redo, GoToMenu,
}

public enum DialogueState
{
    Initialization, Unregistered, Registration,
    NameInput, GenderInput, AgeInput, HeightInput, WeightInput, EmailInput,
    PersonalInformationView, PersonalInformationChange, CoachInformationView,
    Menu, Null
}