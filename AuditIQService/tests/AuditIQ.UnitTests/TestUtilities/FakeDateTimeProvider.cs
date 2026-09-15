using AuditIQ.Application.Abstractions.Time;

namespace AuditIQ.UnitTests.TestUtilities;

public class FakeDateTimeProvider(DateTimeOffset now) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; } = now;
}
