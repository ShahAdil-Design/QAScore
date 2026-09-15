using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Permissions.Dtos;

public sealed record ScreenPermissionDto(UserRole Role, string ScreenKey, bool IsVisible);
