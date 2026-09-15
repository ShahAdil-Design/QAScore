using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 2. Scoped within a Group.</summary>
public class Team : Entity
{
    public required string Name { get; set; }
    public required Guid GroupId { get; set; }
    public Group? Group { get; init; }

    public ICollection<UserTeam> UserTeams { get; init; } = [];
}
