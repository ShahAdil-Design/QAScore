namespace AuditIQ.Domain.Entities;

/// <summary>Many-to-many join — a user can belong to multiple Groups (Section 5, table 2).</summary>
public class UserGroup
{
    public required Guid UserId { get; set; }
    public User? User { get; init; }

    public required Guid GroupId { get; set; }
    public Group? Group { get; init; }
}
