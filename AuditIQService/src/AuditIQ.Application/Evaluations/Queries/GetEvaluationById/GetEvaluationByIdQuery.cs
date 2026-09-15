using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Evaluations.Dtos;

namespace AuditIQ.Application.Evaluations.Queries.GetEvaluationById;

public sealed record GetEvaluationByIdQuery(Guid EvaluationId) : IQuery<EvaluationDetailDto>;
