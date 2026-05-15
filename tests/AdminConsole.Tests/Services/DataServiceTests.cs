using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Services;

public class DataServiceTests : IDisposable, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ConsoleDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public DataServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ConsoleDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ConsoleDbContext(options);
        _dbContext.Database.EnsureCreated();

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task UpdateOrganizationSecurityAsync_OnlyUpdatesCurrentTenant()
    {
        // Arrange
        const int callerOrgId = 1;
        const int otherOrgId = 2;

        _dbContext.Organizations.AddRange(
            new Organization { Id = callerOrgId, Name = "Caller Org", IsMagicLinksEnabled = true },
            new Organization { Id = otherOrgId, Name = "Other Org", IsMagicLinksEnabled = true });
        await _dbContext.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("orgId", callerOrgId.ToString()) }, "mock"));
        _httpContextAccessorMock.Setup(x => x.HttpContext!.User).Returns(user);

        var sut = new DataService(_httpContextAccessorMock.Object, _dbContext);

        // Act
        await sut.UpdateOrganizationSecurityAsync(false);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var callerOrg = await _dbContext.Organizations.FindAsync(callerOrgId);
        var otherOrg = await _dbContext.Organizations.FindAsync(otherOrgId);

        Assert.False(callerOrg!.IsMagicLinksEnabled);
        Assert.True(otherOrg!.IsMagicLinksEnabled);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}