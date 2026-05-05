using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Tests.Factory;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Services;

public class InvitationServiceTests : IDisposable, IAsyncDisposable
{
    private readonly ConsoleDbContext _dbContext;
    private readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));

    private readonly InvitationService _sut;

    public InvitationServiceTests()
    {
        _dbContext = DbContextFactory.Create();
        _sut = new InvitationService(_dbContext, _mailServiceMock.Object, _httpContextAccessorMock.Object, _timeProvider);
    }

    [Fact]
    public async Task ConsumeInviteAsync_WhenInviteIsLive_ReturnsTrueAndDeletesInvite()
    {
        var (_, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(7)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ConsumeInviteAsync(invite);

        Assert.True(result);
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task ConsumeInviteAsync_WhenInviteIsExpired_ReturnsFalseAndDeletesInvite()
    {
        var (_, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30),
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ConsumeInviteAsync(invite);

        Assert.False(result);
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task GetInviteFromRawCodeAsync_WhenInviteIsLive_ReturnsInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(7)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetInviteFromRawCodeAsync(rawCode);

        Assert.NotNull(result);
        Assert.Equal(hashedCode, result.HashedCode);
    }

    [Fact]
    public async Task GetInviteFromRawCodeAsync_WhenInviteIsExpired_ReturnsInviteWithoutDeleting()
    {
        var (rawCode, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30),
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetInviteFromRawCodeAsync(rawCode);

        Assert.NotNull(result);
        Assert.Equal(hashedCode, result.HashedCode);
        Assert.NotEmpty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RemoveExpiredInviteAsync_WhenInviteIsExpired_RemovesAndReturnsTrue()
    {
        var (_, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30),
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.RemoveExpiredInviteAsync(invite);

        Assert.True(result);
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RemoveExpiredInviteAsync_WhenInviteIsLive_LeavesInviteAndReturnsFalse()
    {
        var (_, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(7)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.RemoveExpiredInviteAsync(invite);

        Assert.False(result);
        Assert.NotEmpty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task GetInviteFromRawCodeAsync_WhenCodeDoesNotMatch_ReturnsNull()
    {
        var (_, hashedCode) = GenerateCode();
        var invite = new Invite
        {
            HashedCode = hashedCode,
            ToEmail = "user@example.com",
            TargetOrgId = 1,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpireAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(7)
        };
        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var (otherCode, _) = GenerateCode();

        var result = await _sut.GetInviteFromRawCodeAsync(otherCode);

        Assert.Null(result);
    }

    private static (string rawCode, string hashedCode) GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var rawCode = Convert.ToBase64String(bytes);
        var hashedCode = Convert.ToBase64String(SHA256.HashData(bytes));
        return (rawCode, hashedCode);
    }

    public void Dispose() => _dbContext.Dispose();

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}