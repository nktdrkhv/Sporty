using FluentValidation;

namespace Sporty.Data;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {

        RuleFor(x => x.Name).NotEmpty().Length(2, 64);
        RuleFor(x => x.Age).NotEmpty().InclusiveBetween(10, 100);
        RuleFor(x => x.Height).NotEmpty().InclusiveBetween(100, 250);
        RuleFor(x => x.Weight).NotEmpty().InclusiveBetween(30, 250);
        RuleFor(x => x.Email).NotEmpty().Length(2, 64).EmailAddress();
    }

    //public ValidationResult NameValidate(Person p) => Validate(p, x => x.IncludeProperties(x => x.Name));
}