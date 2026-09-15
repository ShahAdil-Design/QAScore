using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Lookups;

public static class LookupErrors
{
    public static Error CauseCodeNotFound(Guid id) =>
        Error.NotFound("Lookup.CauseCodeNotFound", $"Cause code '{id}' was not found.");

    public static Error DuplicateCauseCodeText(string text) =>
        Error.Conflict("Lookup.DuplicateCauseCodeText", $"A cause code with the text '{text}' already exists.");

    public static Error CommentNotFound(Guid id) =>
        Error.NotFound("Lookup.CommentNotFound", $"Comment '{id}' was not found.");

    public static Error DuplicateCommentText(string text) =>
        Error.Conflict("Lookup.DuplicateCommentText", $"A comment with the text '{text}' already exists.");
}
