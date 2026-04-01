using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Credentials;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Credentials;

public class UserTests : BunitContext
{
    private readonly Mock<IScopedPasswordlessClient> _passwordlessClientMock = new();

    public UserTests()
    {
        Services.AddSingleton(_passwordlessClientMock.Object);

        ComponentFactories.AddStub<AdminConsole.Components.Shared.Credentials>("<div id=\"credentials\"></div>");
    }

    [Fact]
    public void User_ShouldRender_UserIdInSummary()
    {
        // Arrange
        var actual = Render<User>(builder =>
            builder.Add(parameters => parameters.UserId, "123"));

        // Assert
        var actualSummary = actual.Find("#user-details-summary");
        Assert.Contains("123", actualSummary.InnerHtml);
    }

    [Fact]
    public void User_ShouldRender_CredentialsComponent()
    {
        // Arrange
        var actual = Render<User>(builder =>
            builder.Add(parameters => parameters.UserId, "123"));

        // Assert
        Assert.True(actual.HasComponent<Stub<AdminConsole.Components.Shared.Credentials>>());
    }
}