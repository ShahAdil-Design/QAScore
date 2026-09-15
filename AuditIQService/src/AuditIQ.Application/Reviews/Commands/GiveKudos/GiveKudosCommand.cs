using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Reviews.Commands.GiveKudos;

public sealed record GiveKudosCommand(Guid FromUserId, Guid ToUserId, string Message) : ICommand<Guid>;
