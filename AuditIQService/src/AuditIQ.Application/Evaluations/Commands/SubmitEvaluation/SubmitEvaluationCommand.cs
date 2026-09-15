using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.SubmitEvaluation;

public sealed record SubmitEvaluationCommand(Guid EvaluationId, string? EvaluatorNotes) : ICommand;
