using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Lookups.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Queries.GetCommentLibraryItems;

public sealed class GetCommentLibraryItemsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetCommentLibraryItemsQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<Result<IReadOnlyList<LookupItemDto>>> Handle(GetCommentLibraryItemsQuery query, CancellationToken cancellationToken)
    {
        var comments = await db.CommentLibraryItems
            .OrderBy(c => c.Text)
            .Select(c => new LookupItemDto(c.Id, c.Text))
            .ToListAsync(cancellationToken);

        return comments;
    }
}
