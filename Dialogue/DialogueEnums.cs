#pragma warning disable CS0101

namespace Sporty.Dialogue;

public enum DialogueTrigger
{
    SignUp, IsInDb, IsNotInDb,
    MaleInput, FemaleInput, PersonDataInput,
    WatchPersonalInformation, EditPersonalInformation, ConnectWithCoach,
    NameChange, GenderChange, AgeChange, HeightChange, WeightChange, EmailChange,
    BackToMenu,
}

public enum DialogueState
{
    Initialization, Unregistered, Registration,
    NameInput, GenderInput, AgeInput, HeightInput, WeightInput, EmailInput,
    PersonalInformationView, PersonalInformationChange, CoachInfoView,
    Menu
}