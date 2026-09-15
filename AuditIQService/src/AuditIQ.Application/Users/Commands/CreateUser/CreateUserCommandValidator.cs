using FluentValidation;

namespace AuditIQ.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(256);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Role).IsInEnum();
        RuleFor(c => c.EmploymentType).MaximumLength(64);
    }
}
