using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Scorecards.Commands.ArchiveScorecard;

/// <summary>The "delete" side of Scorecard CRUD — a soft delete. Scorecards are never hard-deleted:
/// they're a Temporal Table and existing Evaluations hold a hard FK to a specific version, so
/// removing the row would break historical evaluation data.</summary>
public sealed record ArchiveScorecardCommand(Guid ScorecardId) : ICommand;
