using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 2. Top-level org boundary — Teams are scoped within a Group.</summary>
public class Group : Entity
{
    public required string Name { get; set; }

    public ICollection<Team> Teams { get; init; } = [];
    public ICollection<UserGroup> UserGroups { get; init; } = [];
}
