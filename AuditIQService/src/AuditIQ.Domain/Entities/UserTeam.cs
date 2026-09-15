namespace AuditIQ.Domain.Entities;

/// <summary>Many-to-many join — a user can belong to multiple Teams (Section 5, table 2).</summary>
public class UserTeam
{
    public required Guid UserId { get; set; }
    public User? User { get; init; }

    public required Guid TeamId { get; set; }
    public Team? Team { get; init; }
}
