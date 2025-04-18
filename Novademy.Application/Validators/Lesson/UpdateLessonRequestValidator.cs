using FluentValidation;
using Novademy.Contracts.Requests.Lesson;

namespace Novademy.Application.Validators.Lesson;

public class UpdateLessonRequestValidator  : AbstractValidator<UpdateLessonRequest>
{
    public UpdateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
        
        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Order must be a positive number.");
        
        RuleFor(x => x.Transcript)
            .NotEmpty().WithMessage("Lesson transcript is required.");
    }
}