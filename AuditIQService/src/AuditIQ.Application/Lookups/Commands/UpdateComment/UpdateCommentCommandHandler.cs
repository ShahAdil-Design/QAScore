using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Commands.UpdateComment;

public sealed class UpdateCommentCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateCommentCommand>
{
    public async Task<Result> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await db.CommentLibraryItems.FindAsync([command.Id], cancellationToken);
        if (comment is null)
            return Result.Failure(LookupErrors.CommentNotFound(command.Id));

        var textTaken = await db.CommentLibraryItems.AnyAsync(c => c.Id != command.Id && c.Text == command.Text, cancellationToken);
        if (textTaken)
            return Result.Failure(LookupErrors.DuplicateCommentText(command.Text));

        comment.Text = command.Text;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
