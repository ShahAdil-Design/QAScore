using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.UnlockScorecard;

public sealed record UnlockScorecardCommand(Guid ScorecardId) : ICommand;
