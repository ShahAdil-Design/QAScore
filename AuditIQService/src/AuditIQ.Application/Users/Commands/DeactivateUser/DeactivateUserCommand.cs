using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Users.Commands.DeactivateUser;

/// <summary>Soft toggle only — never a hard delete. Historical Evaluations/CalibrationParticipants
/// hold a hard FK to Users and must stay visible for compliance retention (Section 9), even once
/// the referenced person is deactivated.</summary>
public sealed record DeactivateUserCommand(Guid UserId) : ICommand;
