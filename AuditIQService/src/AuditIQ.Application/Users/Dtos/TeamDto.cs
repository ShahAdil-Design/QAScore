namespace AuditIQ.Application.Users.Dtos;

public sealed record TeamDto(Guid Id, string Name, Guid GroupId, string GroupName);
