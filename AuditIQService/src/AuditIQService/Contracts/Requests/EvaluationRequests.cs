namespace AuditIQ.Api.Contracts.Requests;

public sealed record CreateEvaluationRequest(
    Guid ScorecardId,
    Guid AgentId,
    Guid EvaluatorId,
    Guid? EventTypeId,
    Guid? EventSubTypeId,
    string? Reference,
    DateTimeOffset? EventOccurredAt,
    int? EventDurationSeconds);

// No Score field — score is derived server-side from the chosen answer option's
// configured Value, never accepted from the client.
public sealed record AnswerRequest(Guid QuestionId, string? AnswerValue, string? CauseCode, string? Comment);

public sealed record SaveDraftAnswersRequest(IReadOnlyList<AnswerRequest> Answers);

public sealed record SubmitEvaluationRequest(string? EvaluatorNotes);

public sealed record DisputeEvaluationRequest(string DisputeReason);

public sealed record ResolveDisputeRequest(string ResolutionNotes);
