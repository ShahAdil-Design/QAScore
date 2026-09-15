using AuditIQ.Application.Scorecards.Dtos;

namespace AuditIQ.Application.Evaluations.Dtos;

public sealed record EvaluationAnswerDto(
    Guid QuestionId,
    string QuestionText,
    string SectionName,
    int Weight,
    IReadOnlyList<AnswerOptionDto> AnswerOptions,
    string? AnswerValue,
    string? CauseCode,
    string? Comment,
    decimal? Score);
