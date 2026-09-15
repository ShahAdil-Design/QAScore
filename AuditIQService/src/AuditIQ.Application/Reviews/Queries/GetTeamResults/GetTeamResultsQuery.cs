using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Reviews.Dtos;

namespace AuditIQ.Application.Reviews.Queries.GetTeamResults;

/// <summary>Supervisor view of team-level results (Epic 4). RequestingUserId is the caller's own
/// id — until real SSO/current-user resolution exists (Section 13), every write/read that needs
/// "who is asking" takes it as an explicit trusted parameter, same as Acknowledge/DisputeEvaluation.
/// An Admin may view any team; anyone else must supervise at least one member of it.</summary>
public sealed record GetTeamResultsQuery(Guid TeamId, Guid RequestingUserId) : IQuery<TeamResultsDto>;
