using FluentValidation;

namespace AuditIQ.Application.Lookups.Commands.CreateCauseCode;

public sealed class CreateCauseCodeCommandValidator : AbstractValidator<CreateCauseCodeCommand>
{
    public CreateCauseCodeCommandValidator()
    {
        RuleFor(c => c.Text).NotEmpty().MaximumLength(500);
    }
}
