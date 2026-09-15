using FluentValidation;

namespace AuditIQ.Application.Calibration.Commands.CreateCalibrationList;

public sealed class CreateCalibrationListCommandValidator : AbstractValidator<CreateCalibrationListCommand>
{
    public CreateCalibrationListCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(256);
        RuleFor(c => c.VisibilityScope).NotEmpty().MaximumLength(256);
        RuleFor(c => c.CreatedByUserId).NotEmpty();
    }
}
