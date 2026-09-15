using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.CreateComment;

public sealed record CreateCommentCommand(string Text) : ICommand<Guid>;
