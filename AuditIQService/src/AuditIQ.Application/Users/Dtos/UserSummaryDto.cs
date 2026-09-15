using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Users.Dtos;

public sealed record UserSummaryDto(Guid Id, string DisplayName, string Email, UserRole Role, bool IsActive);
