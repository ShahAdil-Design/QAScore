using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Dashboard.Dtos;

namespace AuditIQ.Application.Dashboard.Queries.GetDashboardSummary;

/// <summary>ScorecardId filters to every version of that scorecard (its ScorecardGroupId), not
/// just the current one — an evaluation may have been scored against an older version.
/// SupervisorId filters to evaluations of that supervisor's direct-report agents.</summary>
public sealed record GetDashboardSummaryQuery(Guid? ScorecardId = null, Guid? SupervisorId = null) : IQuery<DashboardSummaryDto>;
