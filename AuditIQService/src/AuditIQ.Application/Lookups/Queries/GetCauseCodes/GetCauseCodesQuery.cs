using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Lookups.Dtos;

namespace AuditIQ.Application.Lookups.Queries.GetCauseCodes;

public sealed record GetCauseCodesQuery : IQuery<IReadOnlyList<LookupItemDto>>;
