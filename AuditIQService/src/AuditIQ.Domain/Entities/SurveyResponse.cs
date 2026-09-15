using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

public class SurveyResponse : Entity
{
    public required Guid SurveyId { get; set; }
    public Survey? Survey { get; init; }

    public Guid? EvaluationId { get; set; }
    public Evaluation? Evaluation { get; init; }

    public int? Score { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}
