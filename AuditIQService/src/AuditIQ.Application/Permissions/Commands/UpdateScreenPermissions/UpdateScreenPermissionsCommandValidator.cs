using AuditIQ.Domain.Common;
using FluentValidation;

namespace AuditIQ.Application.Permissions.Commands.UpdateScreenPermissions;

public sealed class UpdateScreenPermissionsCommandValidator : AbstractValidator<UpdateScreenPermissionsCommand>
{
    public UpdateScreenPermissionsCommandValidator()
    {
        RuleFor(c => c.Permissions).NotEmpty();

        RuleForEach(c => c.Permissions).ChildRules(permission =>
        {
            permission.RuleFor(p => p.ScreenKey).Must(key => ScreenKeys.All.Contains(key))
                .WithMessage(p => $"'{p.ScreenKey}' is not a known screen.");
        });
    }
}
