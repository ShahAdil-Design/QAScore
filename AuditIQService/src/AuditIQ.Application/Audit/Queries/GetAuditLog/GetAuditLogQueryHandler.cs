using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Audit.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Audit.Queries.GetAuditLog;

public sealed class GetAuditLogQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAuditLogQuery, PagedResult<AuditLogEntryDto>>
{
    public async Task<Result<PagedResult<AuditLogEntryDto>>> Handle(GetAuditLogQuery query, CancellationToken cancellationToken)
    {
        var entries = db.AuditLogEntries.AsQueryable();

        if (query.EntityName is { } entityName)
            entries = entries.Where(a => a.EntityName == entityName);
        if (query.EntityId is { } entityId)
            entries = entries.Where(a => a.EntityId == entityId.ToString());
        if (query.UserId is { } userId)
            entries = entries.Where(a => a.UserId == userId);
        if (query.Action is { } action)
            entries = entries.Where(a => a.Action == action);
        if (query.DateFrom is { } dateFrom)
            entries = entries.Where(a => a.TimestampUtc >= dateFrom);
        if (query.DateTo is { } dateTo)
            entries = entries.Where(a => a.TimestampUtc <= dateTo);

        var totalCount = await entries.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await entries
            .OrderByDescending(a => a.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogEntryDto(
                a.Id,
                a.TimestampUtc,
                a.UserId,
                db.Users.Where(u => u.Id == a.UserId).Select(u => u.DisplayName).FirstOrDefault(),
                a.EntityName,
                a.EntityId,
                a.Action,
                a.Changes))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogEntryDto>(items, page, pageSize, totalCount);
    }
}
