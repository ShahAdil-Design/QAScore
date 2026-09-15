using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Users.Commands.ReactivateUser;

public sealed record ReactivateUserCommand(Guid UserId) : ICommand;
