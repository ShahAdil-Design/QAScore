using AuditIQ.Application.Scorecards.Dtos;

namespace AuditIQ.Application.Calibration.Dtos;

/// <summary>One scorecard question plus the ORIGINAL evaluator's answer to it —
/// what a calibrator re-scores against and compares their own rating to.</summary>
public sealed record CalibrationQuestionDto(
    Guid QuestionId,
    string QuestionText,
    string SectionName,
    int Weight,
    IReadOnlyList<AnswerOptionDto> AnswerOptions,
    string? OriginalAnswerValue,
    string? OriginalCauseCode,
    string? OriginalComment,
    decimal? OriginalScore);

public sealed record CalibrationAnswerDto(Guid QuestionId, string? AnswerValue, string? CauseCode, string? Comment, decimal? Score);

public sealed record CalibrationRatingDto(
    Guid EvaluatorId,
    string EvaluatorName,
    decimal? Score,
    IReadOnlyList<CalibrationAnswerDto> Answers);

/// <summary>
/// Full detail for one item within a calibration list — the context header mirrors
/// what the original evaluation actually was (employee, evaluator, team, event
/// type/sub-type, date/time, duration, reference), the questions carry the original
/// evaluator's answers, and Ratings holds every calibrator who has rated this item
/// so far (there's no fixed participant list — anyone can rate, lazily).
/// </summary>
public sealed record CalibrationItemDetailDto(
    Guid Id,
    Guid CalibrationListId,
    Guid EvaluationId,
    string ScorecardName,
    Guid AgentId,
    string AgentName,
    Guid EvaluatorId,
    string EvaluatorName,
    IReadOnlyList<string> TeamNames,
    string? EventTypeName,
    string? EventSubTypeName,
    DateTimeOffset? EventOccurredAt,
    int? EventDurationSeconds,
    string? Reference,
    decimal? OriginalScore,
    IReadOnlyList<CalibrationQuestionDto> Questions,
    IReadOnlyList<CalibrationRatingDto> Ratings,
    decimal? Variance);
