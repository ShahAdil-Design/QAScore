namespace AuditIQ.Application.Scorecards.Dtos;

public sealed record ScorecardDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string ScorecardType,
    string CategoryName,
    string? Location,
    decimal? TargetPercentage,
    decimal? MaxScore,
    int Version,
    bool IsLocked,
    bool IsArchived,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<string> GroupNames,
    IReadOnlyList<ScorecardQuestionDto> Questions,
    IReadOnlyList<string> TipSheetTitles);
