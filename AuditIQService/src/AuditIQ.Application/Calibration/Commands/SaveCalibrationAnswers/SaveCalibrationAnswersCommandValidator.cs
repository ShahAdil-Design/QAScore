using FluentValidation;

namespace AuditIQ.Application.Calibration.Commands.SaveCalibrationAnswers;

public sealed class SaveCalibrationAnswersCommandValidator : AbstractValidator<SaveCalibrationAnswersCommand>
{
    public SaveCalibrationAnswersCommandValidator()
    {
        RuleFor(c => c.CalibrationListItemId).NotEmpty();
        RuleFor(c => c.EvaluatorId).NotEmpty();
        RuleFor(c => c.Answers).NotEmpty();
        RuleForEach(c => c.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
        });
    }
}
