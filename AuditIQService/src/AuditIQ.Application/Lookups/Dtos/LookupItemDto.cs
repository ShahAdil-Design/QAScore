namespace AuditIQ.Application.Lookups.Dtos;

/// <summary>Shared shape for simple managed lookups (cause codes, canned comments) — just an
/// Id and display text, nothing entity-specific to warrant separate DTOs.</summary>
public sealed record LookupItemDto(Guid Id, string Text);
