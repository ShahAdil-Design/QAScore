using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Dtos;

namespace AuditIQ.Application.Users.Queries.GetTeams;

public sealed record GetTeamsQuery(Guid? GroupId) : IQuery<IReadOnlyList<TeamDto>>;
