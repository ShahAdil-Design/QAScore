using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Dtos;

namespace AuditIQ.Application.Users.Queries.GetGroups;

public sealed record GetGroupsQuery : IQuery<IReadOnlyList<GroupDto>>;
