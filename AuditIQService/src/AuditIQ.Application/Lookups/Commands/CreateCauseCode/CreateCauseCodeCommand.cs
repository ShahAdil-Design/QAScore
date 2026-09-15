using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.CreateCauseCode;

public sealed record CreateCauseCodeCommand(string Text) : ICommand<Guid>;
