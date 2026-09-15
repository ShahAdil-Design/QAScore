using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Permissions.Dtos;

namespace AuditIQ.Application.Permissions.Queries.GetScreenPermissions;

// The full Role x Screen matrix, not just the caller's own role — any authenticated user needs
// it to render their own nav, and the booleans alone don't expose anything sensitive, so there's
// no need for a separate "my screens" projection.
public sealed record GetScreenPermissionsQuery : IQuery<IReadOnlyList<ScreenPermissionDto>>;
