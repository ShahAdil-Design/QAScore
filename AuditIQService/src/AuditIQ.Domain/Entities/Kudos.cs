using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Recognition tracking (Section 3, Epic 4) — not in Section 5's original table list; added here.</summary>
public class Kudos : Entity
{
    public required Guid FromUserId { get; set; }
    public User? FromUser { get; init; }

    public required Guid ToUserId { get; set; }
    public User? ToUser { get; init; }

    public required string Message { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
