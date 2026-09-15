using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Scorecards.Dtos;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecards;

/// <summary>Current, non-archived scorecard versions — for the evaluation queue's scorecard picker.</summary>
public sealed record GetScorecardsQuery(Guid? CategoryId) : IQuery<IReadOnlyList<ScorecardSummaryDto>>;
