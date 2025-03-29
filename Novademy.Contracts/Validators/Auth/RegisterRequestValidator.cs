using FluentValidation;
using Novademy.Contracts.Requests.Auth;

namespace Novademy.Contracts.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(8, 100).WithMessage("Password must be at least 8 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");
        
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
        
        RuleFor(x => x.RoleId)
            .InclusiveBetween(1, 3).WithMessage("RoleId must be between 1 and 3 (1=Admin, 2=Teacher, 3=Student).");
        
        RuleFor(x => x.Group)
            .InclusiveBetween(1, 4).WithMessage("Group must be between 1 and 4.");
        
        RuleFor(x => x.ProfilePictureUrl)
            .Matches(@"^https?://[^\s/$.?#].[^\s]*$").WithMessage("Invalid URL format.")
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }
}