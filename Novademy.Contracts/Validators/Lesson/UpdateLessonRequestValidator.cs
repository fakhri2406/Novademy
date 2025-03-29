using FluentValidation;
using Novademy.Contracts.Requests.Lesson;

namespace Novademy.Contracts.Validators.Lesson;

public class UpdateLessonRequestValidator  : AbstractValidator<UpdateLessonRequest>
{
    public UpdateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
        
        RuleFor(x => x.VideoUrl)
            .NotEmpty().WithMessage("Video URL is required.")
            .Matches(@"^https?://[^\s/$.?#].[^\s]*$").WithMessage("Invalid URL format.");
        
        RuleFor(x => x.Order)
            .GreaterThan(0).WithMessage("Order must be a positive number.");
        
        RuleFor(x => x.ImageUrl)
            .Matches(@"^https?://[^\s/$.?#].[^\s]*$").WithMessage("Invalid URL format.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}