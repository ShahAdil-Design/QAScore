namespace AuditIQ.Api.Contracts.Requests;

public sealed record AnswerOptionRequest(string Label, decimal Value, bool IsFailSection, bool IsFailAll, bool IsNotApplicable);

public sealed record ScorecardQuestionRequest(
    string SectionName,
    string Text,
    int Weight,
    bool IsFailLogic,
    int SortOrder,
    IReadOnlyList<AnswerOptionRequest> AnswerOptions);

// GroupIds empty = "All Groups".
public sealed record CreateScorecardRequest(
    string Name,
    string? Description,
    string ScorecardType,
    Guid CategoryId,
    string? Location,
    decimal? TargetPercentage,
    decimal MaxScore,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<ScorecardQuestionRequest> Questions);

public sealed record UpdateScorecardRequest(
    string Name,
    string? Description,
    string ScorecardType,
    Guid CategoryId,
    string? Location,
    decimal? TargetPercentage,
    decimal MaxScore,
    IReadOnlyList<Guid> GroupIds,
    IReadOnlyList<ScorecardQuestionRequest> Questions);
