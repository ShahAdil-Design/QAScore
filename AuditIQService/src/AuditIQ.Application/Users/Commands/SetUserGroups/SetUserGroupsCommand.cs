using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Users.Commands.SetUserGroups;

public sealed record SetUserGroupsCommand(Guid UserId, IReadOnlyList<Guid> GroupIds) : ICommand;
