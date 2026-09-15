using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Reviews.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Reviews.Queries.GetKudosForUser;

public sealed class GetKudosForUserQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetKudosForUserQuery, IReadOnlyList<KudosDto>>
{
    public async Task<Result<IReadOnlyList<KudosDto>>> Handle(GetKudosForUserQuery query, CancellationToken cancellationToken)
    {
        var results = await db.Kudos
            .Include(k => k.FromUser)
            .Include(k => k.ToUser)
            .Where(k => k.ToUserId == query.UserId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new KudosDto(k.Id, k.FromUser!.DisplayName, k.ToUser!.DisplayName, k.Message, k.CreatedAt))
            .ToListAsync(cancellationToken);

        return results;
    }
}
