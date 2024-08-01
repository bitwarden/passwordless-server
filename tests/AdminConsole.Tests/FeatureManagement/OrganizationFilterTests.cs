using Microsoft.Extensions.Configuration;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.FeatureManagement;

namespace Passwordless.AdminConsole.Tests.FeatureManagement;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

public class OrganizationFeatureFilterTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly OrganizationFeatureFilter _filter;

    public OrganizationFeatureFilterTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _filter = new OrganizationFeatureFilter(_httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_UserIsAuthenticatedAndOrgIdMatches_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test user"),
            new(CustomClaimTypes.OrgId, "123")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var featureFilterEvaluationContext = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { { "Organization", "123" } }
            ).Build()
        };

        // Act
        var result = await _filter.EvaluateAsync(featureFilterEvaluationContext);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EvaluateAsync_UserIsAuthenticatedAndOrgIdDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test user"),
            new(CustomClaimTypes.OrgId, "123")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var featureFilterEvaluationContext = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { { "Organization", "456" } }
            ).Build()
        };

        // Act
        var result = await _filter.EvaluateAsync(featureFilterEvaluationContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_UserIsNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var featureFilterEvaluationContext = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { { "Organization", "123" } }
            ).Build()
        };

        // Act
        var result = await _filter.EvaluateAsync(featureFilterEvaluationContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EvaluateAsync_UserIsAuthenticatedNoOrgIdClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test user")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var featureFilterEvaluationContext = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { { "Organization", "123" } }
            ).Build()
        };

        // Act
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _filter.EvaluateAsync(featureFilterEvaluationContext));

        // Assert
        Assert.Equal("User should have an organization ID claim.", actual.Message);
    }
}