using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Dtos;

namespace AuditIQ.Application.Users.Queries.GetDirectReports;

/// <summary>The people who report to a given supervisor (User.SupervisorId), the foundational
/// query behind any future "my team" scoping — e.g. a Supervisor's dashboard or team-results view.</summary>
public sealed record GetDirectReportsQuery(Guid SupervisorId) : IQuery<IReadOnlyList<UserSummaryDto>>;
