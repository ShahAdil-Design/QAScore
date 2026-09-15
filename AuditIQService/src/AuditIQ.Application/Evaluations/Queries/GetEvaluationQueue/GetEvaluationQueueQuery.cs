using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Evaluations.Dtos;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Evaluations.Queries.GetEvaluationQueue;

/// <summary>
/// Filtered/paged list of existing evaluations (Section 12.B's "evaluation queue").
/// Random-vs-Manual sampling of *unscored* interactions (Section 10, Epic 3) needs
/// the migrated interaction source data from Epic 2, which doesn't exist yet — this
/// queries evaluations already created via CreateEvaluationCommand.
/// </summary>
public sealed record GetEvaluationQueueQuery(
    Guid? AgentId,
    Guid? EvaluatorId,
    Guid? TeamId,
    EvaluationStatus? Status,
    Guid? ScorecardId,
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    int Page = 1,
    int PageSize = 25,
    // Set by the controller for a Supervisor/TeamLead caller — restricts results to their own
    // team (self + direct reports) regardless of what AgentId/EvaluatorId they also pass, so
    // "browse everyone" can never escape their own hierarchy. Null (Admin/QaEvaluator/dev) means
    // no restriction beyond the explicit filters above.
    IReadOnlyList<Guid>? RestrictToAgentIds = null) : IQuery<PagedResult<EvaluationSummaryDto>>;
