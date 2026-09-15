using AuditIQ.Domain.Enums;

namespace AuditIQ.Domain.Entities;

/// <summary>Controls whether a given Role can see a given screen in the frontend nav (composite
/// key: Role + ScreenKey). Screen-visibility only — not a substitute for the API-level
/// authorization policies in AuditIQ.Api.Auth, which remain the real security boundary.</summary>
public class ScreenPermission
{
    public required UserRole Role { get; set; }
    public required string ScreenKey { get; set; }
    public required bool IsVisible { get; set; }
}
