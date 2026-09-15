using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Dtos;

namespace AuditIQ.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDetailDto>;
