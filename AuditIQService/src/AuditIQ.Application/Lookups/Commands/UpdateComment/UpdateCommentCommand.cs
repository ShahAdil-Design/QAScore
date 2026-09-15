using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.UpdateComment;

public sealed record UpdateCommentCommand(Guid Id, string Text) : ICommand;
