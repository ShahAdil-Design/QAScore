using AuditIQ.Domain.Entities;

namespace AuditIQ.Application.Audit.Dtos;

public sealed record AuditLogEntryDto(
    Guid Id,
    DateTimeOffset TimestampUtc,
    Guid? UserId,
    string? UserDisplayName,
    string EntityName,
    string EntityId,
    AuditAction Action,
    string? Changes);
