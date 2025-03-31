using FluentValidation;
using Novademy.Contracts.Requests.Quiz;

namespace Novademy.Contracts.Validators.Quiz;

public class CreateAnswerRequestValidator : AbstractValidator<CreateAnswerRequest>
{
    public CreateAnswerRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Answer text is required.");
        
        RuleFor(x => x.IsCorrect)
            .NotEmpty().WithMessage("Correctness of the answer is required.");
    }
}