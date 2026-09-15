using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecardCategory;

public sealed record UpdateScorecardCategoryCommand(Guid Id, string Name) : ICommand;
