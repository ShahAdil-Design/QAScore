namespace AuditIQ.Api.Contracts.Requests;

// CreatedByUserId is taken from the request body until real SSO/current-user resolution
// exists (Section 13) — same caveat as EvaluationRequests.
public sealed record CreateCalibrationListRequest(string Name, string VisibilityScope, Guid CreatedByUserId);

public sealed record AddEvaluationsToCalibrationListRequest(IReadOnlyList<Guid> EvaluationIds);

// No Score field — score is derived server-side from the chosen answer option's
// configured Value, never accepted from the client.
public sealed record CalibrationAnswerRequest(Guid QuestionId, string? AnswerValue, string? CauseCode, string? Comment);

public sealed record SaveCalibrationAnswersRequest(Guid EvaluatorId, IReadOnlyList<CalibrationAnswerRequest> Answers);
