using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Evaluations.Commands.SaveDraftAnswers;

// No Score field — score is derived server-side from the chosen answer option's
// configured Value (QuestionAnswerOption.cs), never typed in by the evaluator.
public sealed record AnswerInput(Guid QuestionId, string? AnswerValue, string? CauseCode, string? Comment);

public sealed record SaveDraftAnswersCommand(Guid EvaluationId, IReadOnlyList<AnswerInput> Answers) : ICommand;
