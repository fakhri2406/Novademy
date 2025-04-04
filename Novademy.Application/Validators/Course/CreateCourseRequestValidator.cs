using FluentValidation;
using Novademy.Contracts.Requests.Course;

namespace Novademy.Application.Validators.Course;

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
        
        RuleFor(x => x.Subject)
            .IsInEnum().WithMessage("Subject is required.");
    }
}