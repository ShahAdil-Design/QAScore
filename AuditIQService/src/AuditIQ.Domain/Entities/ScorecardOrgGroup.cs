namespace AuditIQ.Domain.Entities;

/// <summary>
/// Which org Groups a scorecard is restricted to — an empty set means "All Groups"
/// (visible/usable everywhere), matching the legacy tool's "All Groups" default.
/// Named ScorecardOrgGroup (not "ScorecardGroup") to avoid clashing with
/// Scorecard.ScorecardGroupId, the unrelated version-lineage key.
/// </summary>
public class ScorecardOrgGroup
{
    public required Guid ScorecardId { get; set; }
    public Scorecard? Scorecard { get; init; }

    public required Guid GroupId { get; set; }
    public Group? Group { get; init; }
}
