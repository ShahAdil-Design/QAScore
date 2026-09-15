using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.DeleteCauseCode;

/// <summary>Safe to hard-delete: EvaluationAnswer.CauseCode stores the text by value at answer
/// time (Section 15 — never collapsed to a reference), so removing a lookup entry doesn't affect
/// any already-recorded answer.</summary>
public sealed record DeleteCauseCodeCommand(Guid Id) : ICommand;
