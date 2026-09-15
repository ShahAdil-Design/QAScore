using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.CreateScorecard;

// GroupIds empty = "All Groups" (visible everywhere) — the legacy tool's default.
public sealed record CreateScorecardCommand(
    string Name,
    string? Description,
    string ScorecardType,
    Guid CategoryId,
    string? Location,
    decimal? TargetPercentage,
    decimal MaxScore,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<ScorecardQuestionInput> Questions) : ICommand<Guid>;
