using AuditIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.UnitTests.TestUtilities;

public static class InMemoryDbContextFactory
{
    public static AuditIQDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AuditIQDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuditIQDbContext(options);
    }
}
