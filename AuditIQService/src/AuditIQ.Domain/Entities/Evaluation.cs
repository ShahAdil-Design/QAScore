using AuditIQ.Domain.Common;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 8 — a Temporal Table. References the exact scorecard version.</summary>
public class Evaluation : Entity, IAuditableEntity
{
    public required Guid ScorecardId { get; set; }
    public Scorecard? Scorecard { get; init; }

    public required Guid AgentId { get; set; }
    public User? Agent { get; init; }

    public required Guid EvaluatorId { get; set; }
    public User? Evaluator { get; init; }

    public Guid? EventTypeId { get; set; }
    public EventType? EventType { get; init; }
    public Guid? EventSubTypeId { get; set; }
    public EventSubType? EventSubType { get; init; }

    public string? Reference { get; set; }
    public required EvaluationStatus Status { get; set; }
    public DateTimeOffset? EventOccurredAt { get; set; }
    public int? EventDurationSeconds { get; set; }

    public string? EvaluatorNotes { get; set; }
    public string? DisputeReason { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }

    /// <summary>Weighted score cached at submit time — denormalized for read-heavy dashboard queries (Section 5).
    /// For evaluations submitted natively in AuditIQ this is the only score. Not set for migrated rows.</summary>
    public decimal? TotalScore { get; set; }

    /// <summary>Scorebuddy's own final score for an evaluation migrated from Scorebuddy, preserved verbatim.
    /// AuditIQ's ScoreCalculator formula (rounding, N/A handling, fail-section semantics) does not necessarily
    /// match Scorebuddy's, so a migrated evaluation must display exactly what Scorebuddy showed rather than a
    /// value recomputed from the migrated answers. Null for evaluations submitted natively in AuditIQ.
    /// Wherever a score is displayed, read <see cref="DisplayScore"/> (LegacyTotalScore ?? TotalScore), never
    /// TotalScore directly, so migrated and native evaluations render consistently.</summary>
    public decimal? LegacyTotalScore { get; set; }

    public decimal? DisplayScore => LegacyTotalScore ?? TotalScore;

    public ICollection<EvaluationAnswer> Answers { get; init; } = [];
    public ICollection<EvaluationAttachment> Attachments { get; init; } = [];
}
