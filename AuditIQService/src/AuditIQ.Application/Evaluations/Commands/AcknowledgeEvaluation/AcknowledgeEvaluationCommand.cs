using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.AcknowledgeEvaluation;

public sealed record AcknowledgeEvaluationCommand(Guid EvaluationId, Guid RequestingAgentId) : ICommand;
