using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Lookups.Commands.DeleteComment;

public sealed record DeleteCommentCommand(Guid Id) : ICommand;
