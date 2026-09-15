using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Evaluations;

public static class EvaluationErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Evaluation.NotFound", $"Evaluation '{id}' was not found.");

    public static Error ScorecardNotFound(Guid id) =>
        Error.NotFound("Evaluation.ScorecardNotFound", $"Scorecard '{id}' was not found.");

    public static Error ScorecardArchived(Guid id) =>
        Error.Validation("Evaluation.ScorecardArchived", $"Scorecard '{id}' is archived and cannot be used for new evaluations.");

    public static Error AgentNotFound(Guid id) =>
        Error.NotFound("Evaluation.AgentNotFound", $"Agent '{id}' was not found.");

    public static Error EvaluatorNotFound(Guid id) =>
        Error.NotFound("Evaluation.EvaluatorNotFound", $"Evaluator '{id}' was not found.");

    public static Error EventTypeNotOnScorecard(Guid eventTypeId) =>
        Error.Validation("Evaluation.EventTypeNotOnScorecard", $"Event type '{eventTypeId}' does not belong to this evaluation's scorecard.");

    public static Error EventSubTypeNotOnEventType(Guid eventSubTypeId) =>
        Error.Validation("Evaluation.EventSubTypeNotOnEventType", $"Event sub-type '{eventSubTypeId}' does not belong to the chosen event type.");

    public static Error QuestionNotOnScorecard(Guid questionId) =>
        Error.Validation("Evaluation.QuestionNotOnScorecard", $"Question '{questionId}' does not belong to this evaluation's scorecard.");

    public static Error AnswerOptionInvalid(Guid questionId, string value) =>
        Error.Validation("Evaluation.AnswerOptionInvalid", $"'{value}' is not a valid answer option for question '{questionId}'.");

    public static readonly Error NotDraft =
        Error.Conflict("Evaluation.NotDraft", "This evaluation is no longer a draft and cannot be edited.");

    // Draft answers can also be edited while Disputed — the evaluator correcting a disputed
    // score reuses the same save-draft-answers path, not a separate command.
    public static readonly Error NotEditable =
        Error.Conflict("Evaluation.NotEditable", "This evaluation cannot be edited in its current state.");

    public static readonly Error IncompleteAnswers =
        Error.Validation("Evaluation.IncompleteAnswers", "Every question must be answered before an evaluation can be submitted.");

    public static readonly Error NotSubmitted =
        Error.Conflict("Evaluation.NotSubmitted", "Only a submitted evaluation can be acknowledged or disputed.");

    public static readonly Error NotDisputed =
        Error.Conflict("Evaluation.NotDisputed", "Only a disputed evaluation can be resolved.");

    public static readonly Error AgentMismatch =
        Error.Forbidden("Evaluation.AgentMismatch", "Only the evaluated agent can acknowledge or dispute this evaluation.");
}
