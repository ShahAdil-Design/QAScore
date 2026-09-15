namespace AuditIQ.Application.Lookups.Dtos;

public sealed record EventSubTypeDto(Guid Id, string Name);

public sealed record EventTypeDto(Guid Id, string Name, IReadOnlyList<EventSubTypeDto> SubTypes);
