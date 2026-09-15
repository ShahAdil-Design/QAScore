using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Lookups.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Queries.GetEventTypes;

public sealed class GetEventTypesQueryHandler(IApplicationDbContext db) : IQueryHandler<GetEventTypesQuery, IReadOnlyList<EventTypeDto>>
{
    public async Task<Result<IReadOnlyList<EventTypeDto>>> Handle(GetEventTypesQuery query, CancellationToken cancellationToken)
    {
        var eventTypes = await db.EventTypes
            .Include(t => t.SubTypes)
            .Where(t => t.ScorecardId == query.ScorecardId)
            .OrderBy(t => t.Name)
            .Select(t => new EventTypeDto(
                t.Id,
                t.Name,
                t.SubTypes.OrderBy(s => s.Name).Select(s => new EventSubTypeDto(s.Id, s.Name)).ToList()))
            .ToListAsync(cancellationToken);

        return eventTypes;
    }
}
