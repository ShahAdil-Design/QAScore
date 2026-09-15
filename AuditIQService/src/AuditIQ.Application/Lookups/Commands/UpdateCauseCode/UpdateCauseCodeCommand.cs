using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.UpdateCauseCode;

public sealed record UpdateCauseCodeCommand(Guid Id, string Text) : ICommand;
