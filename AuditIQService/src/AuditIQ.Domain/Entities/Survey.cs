using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 13. CSAT/NPS survey template. Not part of the current CQRS slice.</summary>
public class Survey : Entity
{
    public required string Name { get; set; }
    public required string Type { get; set; }

    public ICollection<SurveyResponse> Responses { get; init; } = [];
}
