using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Calibration;

public static class CalibrationErrors
{
    public static Error ListNotFound(Guid id) =>
        Error.NotFound("Calibration.ListNotFound", $"Calibration list '{id}' was not found.");

    public static Error ItemNotFound(Guid id) =>
        Error.NotFound("Calibration.ItemNotFound", $"Calibration list item '{id}' was not found.");

    public static Error EvaluationNotFound(Guid id) =>
        Error.NotFound("Calibration.EvaluationNotFound", $"Evaluation '{id}' was not found or is still a draft.");

    public static Error EvaluatorNotFound(Guid id) =>
        Error.NotFound("Calibration.EvaluatorNotFound", $"User '{id}' was not found.");

    public static Error QuestionNotOnScorecard(Guid questionId) =>
        Error.Validation("Calibration.QuestionNotOnScorecard", $"Question '{questionId}' does not belong to this item's scorecard.");

    public static Error AnswerOptionInvalid(Guid questionId, string value) =>
        Error.Validation("Calibration.AnswerOptionInvalid", $"'{value}' is not a valid answer option for question '{questionId}'.");
}
