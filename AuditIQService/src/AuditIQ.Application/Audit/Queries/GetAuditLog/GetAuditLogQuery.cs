using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Audit.Dtos;
using AuditIQ.Domain.Entities;

namespace AuditIQ.Application.Audit.Queries.GetAuditLog;

/// <summary>Filtered/paged browse of AuditLogEntries — "who did X" for the entities marked
/// IAuditableEntity (currently User, Scorecard). Admin-only, enforced at the controller.</summary>
public sealed record GetAuditLogQuery(
    string? EntityName,
    Guid? EntityId,
    Guid? UserId,
    AuditAction? Action,
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    int Page = 1,
    int PageSize = 25) : IQuery<PagedResult<AuditLogEntryDto>>;
