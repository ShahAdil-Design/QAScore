using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.DeleteScorecardCategory;

public sealed record DeleteScorecardCategoryCommand(Guid Id) : ICommand;
