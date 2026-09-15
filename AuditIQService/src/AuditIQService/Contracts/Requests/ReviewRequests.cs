namespace AuditIQ.Api.Contracts.Requests;

// FromUserId is taken from the request body until real SSO/current-user resolution
// exists (Section 13) — same caveat as EvaluationRequests.
public sealed record GiveKudosRequest(Guid FromUserId, Guid ToUserId, string Message);
