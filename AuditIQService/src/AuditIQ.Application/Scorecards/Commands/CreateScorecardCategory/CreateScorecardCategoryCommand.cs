using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.CreateScorecardCategory;

public sealed record CreateScorecardCategoryCommand(string Name) : ICommand<Guid>;
