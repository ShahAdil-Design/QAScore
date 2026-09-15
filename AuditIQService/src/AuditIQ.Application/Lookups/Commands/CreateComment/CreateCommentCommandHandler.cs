using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Commands.CreateComment;

public sealed class CreateCommentCommandHandler(IApplicationDbContext db) : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var exists = await db.CommentLibraryItems.AnyAsync(c => c.Text == command.Text, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(LookupErrors.DuplicateCommentText(command.Text));

        var comment = new CommentLibraryItem { Text = command.Text };

        db.CommentLibraryItems.Add(comment);
        await db.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
