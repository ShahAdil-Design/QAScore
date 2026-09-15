using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Scorecards.Dtos;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecardById;

public sealed record GetScorecardByIdQuery(Guid ScorecardId) : IQuery<ScorecardDetailDto>;
