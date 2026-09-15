using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Users.Commands.SetUserTeams;

/// <summary>Replaces a user's team memberships wholesale — matches how the admin UI submits the
/// full desired set each time, same pattern as UpdateScorecard's question replacement.</summary>
public sealed record SetUserTeamsCommand(Guid UserId, IReadOnlyList<Guid> TeamIds) : ICommand;
