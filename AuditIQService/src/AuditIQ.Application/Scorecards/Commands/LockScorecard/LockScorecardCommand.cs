using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.LockScorecard;

public sealed record LockScorecardCommand(Guid ScorecardId) : ICommand;
