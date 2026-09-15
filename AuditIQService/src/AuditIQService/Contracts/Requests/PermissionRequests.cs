using AuditIQ.Domain.Enums;

namespace AuditIQ.Api.Contracts.Requests;

public sealed record ScreenPermissionRequest(UserRole Role, string ScreenKey, bool IsVisible);

public sealed record UpdateScreenPermissionsRequest(IReadOnlyList<ScreenPermissionRequest> Permissions);
