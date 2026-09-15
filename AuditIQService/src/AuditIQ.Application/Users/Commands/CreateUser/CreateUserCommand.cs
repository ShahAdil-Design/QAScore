using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string DisplayName,
    string Email,
    UserRole Role,
    string? EmploymentType,
    IReadOnlyList<Guid> TeamIds,
    IReadOnlyList<Guid> GroupIds) : ICommand<Guid>;
