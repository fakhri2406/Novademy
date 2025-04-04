using FluentValidation;
using Novademy.Contracts.Requests.Package;

namespace Novademy.Application.Validators.Package;

public class UpdatePackageRequestValidator  : AbstractValidator<UpdatePackageRequest>
{
    public UpdatePackageRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(5, 100).WithMessage("Title must be between 5 and 100 characters.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(10, 500).WithMessage("Description must be between 10 and 500 characters.");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be a positive value.");
        
        RuleFor(x => x.CourseIds)
            .NotEmpty().WithMessage("At least one Course ID is required.")
            .Must(ids => ids != null && ids.Any()).WithMessage("At least one Course ID is required.");
    }
}