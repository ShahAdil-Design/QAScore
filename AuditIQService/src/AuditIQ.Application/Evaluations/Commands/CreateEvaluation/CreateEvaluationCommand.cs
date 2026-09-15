using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.CreateEvaluation;

public sealed record CreateEvaluationCommand(
    Guid ScorecardId,
    Guid AgentId,
    Guid EvaluatorId,
    Guid? EventTypeId,
    Guid? EventSubTypeId,
    string? Reference,
    DateTimeOffset? EventOccurredAt,
    int? EventDurationSeconds) : ICommand<Guid>;
