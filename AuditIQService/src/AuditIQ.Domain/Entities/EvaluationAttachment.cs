using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 14. Cloud Storage path reference — never a binary blob (ADR-10).</summary>
public class EvaluationAttachment : Entity
{
    public required Guid EvaluationId { get; set; }
    public Evaluation? Evaluation { get; init; }

    public required string FileName { get; set; }
    public required string StoragePath { get; set; }
    public long SizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
