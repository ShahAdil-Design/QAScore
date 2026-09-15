using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 11. Top level of the two-level interaction taxonomy — scoped to a
/// single scorecard, not shared: Scorebuddy's own event/sub-event ids are minted per scorecard
/// (two scorecards' "Manage Collections" are different records with different ids and different
/// sub-event lists), and a scorecard can have none at all (event type is optional per
/// evaluation). Belongs to the scorecard's current version — event types aren't versioned
/// separately from the scorecard itself.</summary>
public class EventType : Entity
{
    public required Guid ScorecardId { get; set; }
    public Scorecard? Scorecard { get; init; }

    public required string Name { get; set; }

    public ICollection<EventSubType> SubTypes { get; init; } = [];
}
