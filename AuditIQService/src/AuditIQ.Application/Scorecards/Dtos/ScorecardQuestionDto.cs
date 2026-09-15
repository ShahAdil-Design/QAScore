namespace AuditIQ.Application.Scorecards.Dtos;

public sealed record AnswerOptionDto(string Label, decimal Value, bool IsFailSection, bool IsFailAll, bool IsNotApplicable);

public sealed record ScorecardQuestionDto(
    Guid Id,
    string SectionName,
    string Text,
    int Weight,
    bool IsFailLogic,
    int SortOrder,
    IReadOnlyList<AnswerOptionDto> AnswerOptions,
    decimal MaxScore);
