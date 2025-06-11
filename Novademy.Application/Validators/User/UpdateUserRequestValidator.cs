using FluentValidation;
using Novademy.Contracts.Requests.User;

namespace Novademy.Application.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0?\d{9}$").WithMessage("Phone number must be 9 or 10 digits, optionally starting with 0.");
    }
}