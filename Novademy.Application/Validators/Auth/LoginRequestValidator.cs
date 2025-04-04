using FluentValidation;
using Novademy.Contracts.Requests.Auth;

namespace Novademy.Application.Validators.Auth;

public class LoginRequestValidator  : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}