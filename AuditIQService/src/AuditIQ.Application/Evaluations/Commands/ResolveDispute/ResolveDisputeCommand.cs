using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.ResolveDispute;

public sealed record ResolveDisputeCommand(Guid EvaluationId, string ResolutionNotes) : ICommand;
