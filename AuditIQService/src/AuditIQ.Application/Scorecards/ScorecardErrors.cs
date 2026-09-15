using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Scorecards;

public static class ScorecardErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Scorecard.NotFound", $"Scorecard '{id}' was not found.");

    public static Error CategoryNotFound(Guid id) =>
        Error.NotFound("Scorecard.CategoryNotFound", $"Scorecard category '{id}' was not found.");

    public static Error GroupNotFound(Guid id) =>
        Error.NotFound("Scorecard.GroupNotFound", $"Group '{id}' was not found.");

    public static Error DuplicateCategoryName(string name) =>
        Error.Conflict("Scorecard.DuplicateCategoryName", $"A scorecard category named '{name}' already exists.");

    public static Error CategoryInUse(Guid id) =>
        Error.Conflict("Scorecard.CategoryInUse", $"Scorecard category '{id}' is used by at least one scorecard and cannot be deleted.");

    public static Error Archived(Guid id) =>
        Error.Validation("Scorecard.Archived", $"Scorecard '{id}' is archived and cannot be modified.");

    public static Error Locked(Guid id) =>
        Error.Validation("Scorecard.Locked", $"Scorecard '{id}' is locked and cannot be modified. Unlock it first.");

    public static readonly Error AlreadyArchived =
        Error.Conflict("Scorecard.AlreadyArchived", "This scorecard is already archived.");

    public static readonly Error AlreadyLocked =
        Error.Conflict("Scorecard.AlreadyLocked", "This scorecard is already locked.");

    public static readonly Error NotLocked =
        Error.Conflict("Scorecard.NotLocked", "This scorecard is not locked.");
}
