using FluentValidation;
using Novademy.Contracts.Requests.Subscription;

namespace Novademy.Application.Validators.Subscription;

public class SubscribeRequestValidator : AbstractValidator<SubscriptionRequest>
{
    public SubscribeRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Package ID is required.");
    }
}