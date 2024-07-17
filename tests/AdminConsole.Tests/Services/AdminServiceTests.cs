using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Services;

public class AdminServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IPasswordlessClient> _passwordlessClientMock;
    private readonly ConsoleDbContext _dbContext;
    private readonly Mock<ILogger<AdminService>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _passwordlessClientMock = new Mock<IPasswordlessClient>();
        _loggerMock = new Mock<ILogger<AdminService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var options = new DbContextOptionsBuilder<ConsoleDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _dbContext = new ConsoleDbContext(options);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("orgId", "1"), }, "mock"));
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(user);

        _sut = new AdminService(_passwordlessClientMock.Object, _dbContext, _httpContextAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CanDisableMagicLinksAsync_ShouldReturnFalse_WhenNoCredentials()
    {
        // Arrange
        var admins = _fixture.Build<ConsoleAdmin>()
            .With(x => x.OrganizationId, 1)
            .Without(x => x.Organization)
            .CreateMany(2);

        _dbContext.Users.AddRange(admins);
        _dbContext.SaveChanges();

        _passwordlessClientMock.Setup(x => x.ListCredentialsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Credential>());

        // Act
        var result = await _sut.CanDisableMagicLinksAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanDisableMagicLinksAsync_ShouldReturnTrue_WhenAllAdminsHaveCredentials()
    {
        // Arrange
        const string adminId1 = "admin1";
        const string adminId2 = "admin2";

        var users = new List<ConsoleAdmin>
        {
            _fixture.Build<ConsoleAdmin>()
                .With(x => x.Id, adminId1)
                .Without(x => x.Organization).Create(),
            _fixture.Build<ConsoleAdmin>()
                .With(x => x.Id, adminId2)
                .Without(x => x.Organization).Create()
        };

        _dbContext.Users.AddRange(users);
        _dbContext.SaveChanges();

        _passwordlessClientMock.Setup(x => x.ListCredentialsAsync(It.Is<string>(p => p == adminId1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Credential> { _fixture.Create<Credential>() });

        _passwordlessClientMock.Setup(x => x.ListCredentialsAsync(It.Is<string>(p => p == adminId2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Credential> { _fixture.Create<Credential>() });

        var service = new AdminService(_passwordlessClientMock.Object, _dbContext, _httpContextAccessorMock.Object, _loggerMock.Object);

        // Act
        var result = await service.CanDisableMagicLinksAsync();

        // Assert
        Assert.True(result);
    }
}