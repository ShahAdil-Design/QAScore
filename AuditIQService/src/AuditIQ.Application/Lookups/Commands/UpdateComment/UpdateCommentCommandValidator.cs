using FluentValidation;

namespace AuditIQ.Application.Lookups.Commands.UpdateComment;

public sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Text).NotEmpty().MaximumLength(500);
    }
}
