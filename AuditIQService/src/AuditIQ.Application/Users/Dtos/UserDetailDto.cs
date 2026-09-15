using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Users.Dtos;

public sealed record TeamMembershipDto(Guid TeamId, string TeamName, Guid GroupId, string GroupName);

public sealed record GroupMembershipDto(Guid GroupId, string GroupName);

public sealed record UserDetailDto(
    Guid Id,
    string DisplayName,
    string Email,
    UserRole Role,
    bool IsActive,
    string? EmploymentType,
    string? Notes,
    IReadOnlyList<TeamMembershipDto> Teams,
    IReadOnlyList<GroupMembershipDto> Groups);
