using FluentValidation;

namespace AuditIQ.Application.Evaluations.Commands.SaveDraftAnswers;

public sealed class SaveDraftAnswersCommandValidator : AbstractValidator<SaveDraftAnswersCommand>
{
    public SaveDraftAnswersCommandValidator()
    {
        RuleFor(c => c.EvaluationId).NotEmpty();
        RuleFor(c => c.Answers).NotEmpty();
        RuleForEach(c => c.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
        });
    }
}
