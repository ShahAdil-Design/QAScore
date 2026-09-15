namespace AuditIQ.Application.Reviews.Dtos;

public sealed record KudosDto(Guid Id, string FromUserName, string ToUserName, string Message, DateTimeOffset CreatedAt);
