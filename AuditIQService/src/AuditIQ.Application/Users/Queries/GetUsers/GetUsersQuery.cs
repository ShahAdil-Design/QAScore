using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Dtos;
using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Users.Queries.GetUsers;

/// <summary>Active-user lookup for pickers (evaluation agent/evaluator, calibration reviewers,
/// kudos recipients) — not the full Phase-2 Staff admin CRUD.</summary>
public sealed record GetUsersQuery(UserRole? Role, bool IncludeInactive = false, Guid? GroupId = null) : IQuery<IReadOnlyList<UserSummaryDto>>;
