using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Users;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static Error DuplicateEmail(string email) =>
        Error.Conflict("User.DuplicateEmail", $"A user with email '{email}' already exists.");

    public static Error TeamNotFound(Guid id) =>
        Error.NotFound("User.TeamNotFound", $"Team '{id}' was not found.");

    public static Error GroupNotFound(Guid id) =>
        Error.NotFound("User.GroupNotFound", $"Group '{id}' was not found.");

    public static readonly Error AlreadyDeactivated =
        Error.Conflict("User.AlreadyDeactivated", "This user is already deactivated.");

    public static readonly Error AlreadyActive =
        Error.Conflict("User.AlreadyActive", "This user is already active.");

    /// <summary>The caller authenticated successfully (their identity is real) but has no
    /// matching AuditIQ user record — SSO proves identity, but AuditIQ's own Users table is what
    /// actually grants access. Provisioning them via the Staff page is what's missing.</summary>
    public static readonly Error NotProvisioned =
        Error.Forbidden("User.NotProvisioned", "Your account isn't provisioned in AuditIQ yet — ask an admin to add you on the Staff page.");

    public static readonly Error NotAuthorizedForUser =
        Error.Forbidden("User.NotAuthorizedForUser", "You can only view your own direct reports.");

    public static readonly Error DirectoryUnavailable =
        Error.Failure("User.DirectoryUnavailable", "Couldn't reach the directory service to look up accounts — check the email manually instead.");
}
