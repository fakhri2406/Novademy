using FluentValidation;
using Novademy.Contracts.Requests.Course;

namespace Novademy.Contracts.Validators.Course;

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
        
        RuleFor(x => x.Subject)
            .IsInEnum().WithMessage("Subject is required.");
    }
}