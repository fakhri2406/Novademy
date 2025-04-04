using FluentValidation;
using Novademy.Contracts.Requests.Quiz;

namespace Novademy.Application.Validators.Quiz;

public class CreateQuizRequestValidator : AbstractValidator<CreateQuizRequest>
{
    public CreateQuizRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Lesson ID is required.");
    }
}