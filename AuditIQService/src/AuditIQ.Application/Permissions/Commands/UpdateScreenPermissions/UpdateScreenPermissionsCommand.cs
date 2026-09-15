using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Permissions.Commands.UpdateScreenPermissions;

public sealed record ScreenPermissionInput(UserRole Role, string ScreenKey, bool IsVisible);

// Replaces the entire Role x Screen matrix in one call — it's a small, fixed-size grid (roles x
// screens), so a full-replacement PUT is simpler and safer than per-cell PATCH endpoints (no
// risk of the stored matrix ending up with stale rows for a screen/role combo the UI forgot to
// send).
public sealed record UpdateScreenPermissionsCommand(IReadOnlyList<ScreenPermissionInput> Permissions) : ICommand;
