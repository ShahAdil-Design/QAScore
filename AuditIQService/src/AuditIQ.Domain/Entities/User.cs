using AuditIQ.Domain.Common;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 1. Linked to SSO identity — no local password/credential storage.</summary>
public class User : Entity, IAuditableEntity
{
    public required string SsoSubjectId { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ExternalId { get; set; }
    public string? EmploymentType { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }

    // Person-to-person reporting link, mirroring Scorebuddy's staff.supervisor_id directly
    // (Scorebuddy has no team-owner field — team "ownership" is only ever implied via this).
    public Guid? SupervisorId { get; set; }
    public User? Supervisor { get; init; }
    public ICollection<User> DirectReports { get; init; } = [];

    public ICollection<UserGroup> UserGroups { get; init; } = [];
    public ICollection<UserTeam> UserTeams { get; init; } = [];
}
