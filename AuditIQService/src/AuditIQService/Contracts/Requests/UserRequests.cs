using AuditIQ.Domain.Enums;

namespace AuditIQ.Api.Contracts.Requests;

public sealed record CreateUserRequest(
    string DisplayName,
    string Email,
    UserRole Role,
    string? EmploymentType,
    IReadOnlyList<Guid> TeamIds,
    IReadOnlyList<Guid> GroupIds);

public sealed record UpdateUserRequest(
    string DisplayName,
    string Email,
    UserRole Role,
    string? EmploymentType,
    string? Notes);

public sealed record SetUserTeamsRequest(IReadOnlyList<Guid> TeamIds);

public sealed record SetUserGroupsRequest(IReadOnlyList<Guid> GroupIds);
