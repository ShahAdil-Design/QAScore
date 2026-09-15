using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 10. Managed lookup feeding the scoring form's Comment dropdown.</summary>
public class CommentLibraryItem : Entity
{
    public required string Text { get; set; }
}
