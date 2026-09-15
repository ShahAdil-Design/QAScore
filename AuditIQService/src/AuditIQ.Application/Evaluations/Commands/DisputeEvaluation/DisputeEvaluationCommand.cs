using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.DisputeEvaluation;

public sealed record DisputeEvaluationCommand(Guid EvaluationId, Guid RequestingAgentId, string DisputeReason) : ICommand;
