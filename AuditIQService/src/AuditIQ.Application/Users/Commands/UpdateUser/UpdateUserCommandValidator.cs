using FluentValidation;

namespace AuditIQ.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(256);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Role).IsInEnum();
        RuleFor(c => c.EmploymentType).MaximumLength(64);
        RuleFor(c => c.Notes).MaximumLength(2000);
    }
}
