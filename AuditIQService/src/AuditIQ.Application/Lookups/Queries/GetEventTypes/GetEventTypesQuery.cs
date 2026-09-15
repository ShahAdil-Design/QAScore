using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Lookups.Dtos;

namespace AuditIQ.Application.Lookups.Queries.GetEventTypes;

/// <summary>Event types are scoped to a single scorecard (see EventType.cs) — a scorecard with
/// none configured in Scorebuddy returns an empty list, meaning event type simply isn't
/// applicable there.</summary>
public sealed record GetEventTypesQuery(Guid ScorecardId) : IQuery<IReadOnlyList<EventTypeDto>>;
