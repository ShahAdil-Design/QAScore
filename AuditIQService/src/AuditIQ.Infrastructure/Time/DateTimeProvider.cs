using AuditIQ.Application.Abstractions.Time;

namespace AuditIQ.Infrastructure.Time;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
