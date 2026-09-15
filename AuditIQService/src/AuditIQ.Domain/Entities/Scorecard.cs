using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// Section 5, table 3 — a Temporal Table (row-level history is automatic; see
/// AuditIQDbContext's .IsTemporal() config). Scorecards are never mutated once
/// published: editing creates a new row sharing the same ScorecardGroupId with
/// an incremented Version, and IsCurrentVersion flips to the new row.
/// </summary>
public class Scorecard : Entity, IAuditableEntity
{
    public required Guid ScorecardGroupId { get; set; }
    public required int Version { get; set; }
    public required bool IsCurrentVersion { get; set; }

    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string ScorecardType { get; set; }
    public required Guid CategoryId { get; set; }
    public ScorecardCategory? Category { get; init; }
    public string? Location { get; set; }
    public decimal? TargetPercentage { get; set; }

    /// <summary>The denominator for this version's total-score percentage — Scorebuddy's own
    /// scoring divides by an independently-configured "max score" per version, not by the sum
    /// of question weights (the two only happen to coincide by convention on most scorecards;
    /// confirmed empirically against real migrated evaluation data that they can genuinely
    /// differ). Nullable only for backward compatibility with scorecards that predate this
    /// field — ScoreCalculator falls back to the weight sum when null. Required (see
    /// CreateScorecardCommandValidator) for every scorecard created going forward, so native
    /// AuditIQ scoring follows the same formula as Scorebuddy.</summary>
    public decimal? MaxScore { get; set; }

    public bool IsLocked { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ScorecardQuestion> Questions { get; init; } = [];
    public ICollection<ScorecardTipSheet> TipSheets { get; init; } = [];
    public ICollection<ScorecardOrgGroup> OrgGroups { get; init; } = [];
}
