using FluentValidation;

namespace AuditIQ.Application.Reviews.Commands.GiveKudos;

public sealed class GiveKudosCommandValidator : AbstractValidator<GiveKudosCommand>
{
    public GiveKudosCommandValidator()
    {
        RuleFor(c => c.FromUserId).NotEmpty();
        RuleFor(c => c.ToUserId).NotEmpty();
        RuleFor(c => c.Message).NotEmpty().MaximumLength(1000);
    }
}
