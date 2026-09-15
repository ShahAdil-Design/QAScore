using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Lookups.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler(IApplicationDbContext db) : ICommandHandler<DeleteCommentCommand>
{
    public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await db.CommentLibraryItems.FindAsync([command.Id], cancellationToken);
        if (comment is null)
            return Result.Failure(LookupErrors.CommentNotFound(command.Id));

        db.CommentLibraryItems.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
