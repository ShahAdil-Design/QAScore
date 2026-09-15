using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Reviews.Dtos;

namespace AuditIQ.Application.Reviews.Queries.GetKudosForUser;

public sealed record GetKudosForUserQuery(Guid UserId) : IQuery<IReadOnlyList<KudosDto>>;
