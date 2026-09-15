using AuditIQ.Domain.Entities;

namespace AuditIQ.Application.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Message,
    Guid? RelatedEvaluationId,
    Guid? RelatedCalibrationListId,
    DateTimeOffset CreatedAtUtc,
    bool IsRead);
