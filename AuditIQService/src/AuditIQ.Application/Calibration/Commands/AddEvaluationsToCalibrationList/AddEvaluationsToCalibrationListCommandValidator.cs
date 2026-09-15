using FluentValidation;

namespace AuditIQ.Application.Calibration.Commands.AddEvaluationsToCalibrationList;

public sealed class AddEvaluationsToCalibrationListCommandValidator : AbstractValidator<AddEvaluationsToCalibrationListCommand>
{
    public AddEvaluationsToCalibrationListCommandValidator()
    {
        RuleFor(c => c.CalibrationListId).NotEmpty();
        RuleFor(c => c.EvaluationIds).NotEmpty();
    }
}
