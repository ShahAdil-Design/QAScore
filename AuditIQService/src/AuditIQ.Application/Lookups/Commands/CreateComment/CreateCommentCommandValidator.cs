using FluentValidation;

namespace AuditIQ.Application.Lookups.Commands.CreateComment;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(c => c.Text).NotEmpty().MaximumLength(500);
    }
}
