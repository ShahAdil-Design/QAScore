namespace AuditIQ.Application.Scorecards.Dtos;

public sealed record ScorecardSummaryDto(
    Guid Id,
    string Name,
    string CategoryName,
    string? Location,
    decimal? TargetPercentage,
    decimal? MaxScore,
    int Version,
    bool IsLocked,
    int QuestionCount,
    IReadOnlyList<string> GroupNames);
