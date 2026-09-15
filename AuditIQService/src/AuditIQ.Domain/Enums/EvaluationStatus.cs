namespace AuditIQ.Domain.Enums;

/// <summary>
/// Draft -&gt; Submitted -&gt; Acknowledged, or Submitted -&gt; Disputed -&gt; Resolved
/// (Section 5, table 8; Epic 3/4 dispute-review workflow).
/// </summary>
public enum EvaluationStatus
{
    Draft,
    Submitted,
    Acknowledged,
    Disputed,
    Resolved,
}
