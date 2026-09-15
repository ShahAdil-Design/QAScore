using AuditIQ.Application.Reviews.Commands.GiveKudos;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.UnitTests.TestUtilities;
using Xunit;

namespace AuditIQ.UnitTests.Reviews;

public class GiveKudosCommandHandlerTests
{
    [Fact]
    public async Task GivenTwoDistinctUsers_WhenGivingKudos_ThenKudosIsRecorded()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var from = new User { SsoSubjectId = "from", DisplayName = "Chris", Email = "chris@test.com", Role = UserRole.Supervisor };
        var to = new User { SsoSubjectId = "to", DisplayName = "Maya", Email = "maya@test.com", Role = UserRole.Agent };
        db.Users.AddRange(from, to);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GiveKudosCommandHandler(db, new FakeDateTimeProvider(DateTimeOffset.UtcNow));
        var result = await handler.Handle(new GiveKudosCommand(from.Id, to.Id, "Great call!"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var kudos = await db.Kudos.FindAsync([result.Value], CancellationToken.None);
        Assert.NotNull(kudos);
        Assert.Equal("Great call!", kudos.Message);
    }

    [Fact]
    public async Task GivenTheSameUserAsFromAndTo_WhenGivingKudos_ThenResultIsFailure()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = new User { SsoSubjectId = "self", DisplayName = "Maya", Email = "maya@test.com", Role = UserRole.Agent };
        db.Users.Add(user);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GiveKudosCommandHandler(db, new FakeDateTimeProvider(DateTimeOffset.UtcNow));
        var result = await handler.Handle(new GiveKudosCommand(user.Id, user.Id, "Go me"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Review.CannotKudosSelf", result.FirstError.Code);
    }
}
