using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Reviews;

public static class ReviewErrors
{
    public static Error UserNotFound(Guid id) =>
        Error.NotFound("Review.UserNotFound", $"User '{id}' was not found.");

    public static Error TeamNotFound(Guid id) =>
        Error.NotFound("Review.TeamNotFound", $"Team '{id}' was not found.");

    public static readonly Error NotAuthorizedForTeam =
        Error.Forbidden("Review.NotAuthorizedForTeam", "You do not supervise any member of this team.");

    public static readonly Error CannotKudosSelf =
        Error.Validation("Review.CannotKudosSelf", "You cannot give kudos to yourself.");
}
