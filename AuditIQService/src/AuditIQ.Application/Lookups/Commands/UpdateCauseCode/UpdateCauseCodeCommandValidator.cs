using FluentValidation;

namespace AuditIQ.Application.Lookups.Commands.UpdateCauseCode;

public sealed class UpdateCauseCodeCommandValidator : AbstractValidator<UpdateCauseCodeCommand>
{
    public UpdateCauseCodeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Text).NotEmpty().MaximumLength(500);
    }
}
