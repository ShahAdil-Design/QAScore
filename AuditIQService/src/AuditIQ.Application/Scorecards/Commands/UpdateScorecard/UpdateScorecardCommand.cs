using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecard;

/// <summary>
/// Scorecards are never mutated in place once created (Scorecard.cs) — this always produces
/// a new version row sharing the same ScorecardGroupId, flips IsCurrentVersion onto it, and
/// returns its Id. Existing evaluations keep pointing at the prior version's questions, so
/// historical scoring is unaffected by later edits. GroupIds empty = "All Groups".
/// </summary>
public sealed record UpdateScorecardCommand(
    Guid ScorecardId,
    string Name,
    string? Description,
    string ScorecardType,
    Guid CategoryId,
    string? Location,
    decimal? TargetPercentage,
    decimal MaxScore,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<ScorecardQuestionInput> Questions) : ICommand<Guid>;
