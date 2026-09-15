using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Calibration.Commands.SaveCalibrationAnswers;

// No Score field — score is derived server-side from the chosen answer option's
// configured Value, never accepted from the client.
public sealed record CalibrationAnswerInput(Guid QuestionId, string? AnswerValue, string? CauseCode, string? Comment);

public sealed record SaveCalibrationAnswersCommand(Guid CalibrationListItemId, Guid EvaluatorId, IReadOnlyList<CalibrationAnswerInput> Answers) : ICommand;
