using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string DisplayName,
    string Email,
    UserRole Role,
    string? EmploymentType,
    string? Notes) : ICommand;
