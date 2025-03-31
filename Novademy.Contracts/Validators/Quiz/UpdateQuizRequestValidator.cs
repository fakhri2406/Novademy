using FluentValidation;
using Novademy.Contracts.Requests.Quiz;

namespace Novademy.Contracts.Validators.Quiz;

public class UpdateQuizRequestValidator : AbstractValidator<UpdateQuizRequest>
{
    public UpdateQuizRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
    }
}