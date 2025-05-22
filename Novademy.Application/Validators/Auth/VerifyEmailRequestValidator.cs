using FluentValidation;
using Novademy.Contracts.Requests.Auth;

namespace Novademy.Application.Validators.Auth;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.UserId).
            NotEmpty().WithMessage("User ID is required.");
        
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(4)
            .Matches("^[0-9]{4}$").WithMessage("Invalid code format.");
    }
}