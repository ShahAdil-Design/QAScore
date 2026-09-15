using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Lookups.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Queries.GetCauseCodes;

public sealed class GetCauseCodesQueryHandler(IApplicationDbContext db) : IQueryHandler<GetCauseCodesQuery, IReadOnlyList<LookupItemDto>>
{
    public async Task<Result<IReadOnlyList<LookupItemDto>>> Handle(GetCauseCodesQuery query, CancellationToken cancellationToken)
    {
        var causeCodes = await db.CauseCodes
            .OrderBy(c => c.Text)
            .Select(c => new LookupItemDto(c.Id, c.Text))
            .ToListAsync(cancellationToken);

        return causeCodes;
    }
}
