using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings.SettingsComponents;

public class DeleteApplicationSectionTests : TestContext
{
    private readonly Mock<IApplicationService> _appServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILogger<DeleteApplicationSection>> _loggerMock = new();

    public DeleteApplicationSectionTests()
    {
        // Arrange
        Services.AddSingleton(_appServiceMock.Object);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_loggerMock.Object);
    }

    [Fact]
    public void Renders_CannotDeleteImmediatelyParagraphs_WhenCannotDeleteImmediately()
    {
        // Arrange
        var application = new Application { Id = "app1", Name = "App1" };
        _appServiceMock.Setup(x =>
                x.CanDeleteApplicationImmediatelyAsync(It.Is<string>(p => p == application.Id)))
            .ReturnsAsync(false);

        // Act
        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Assert
        var actual1 = cut.Find("#cannot-delete-immediately-reason");
        Assert.NotNull(actual1);
        var actual2 = cut.Find("#cannot-delete-immediately-impact");
        Assert.NotNull(actual2);
    }

    [Fact]
    public void DoesNotRender_CannotDeleteImmediatelyParagraphs_WhenCanDeleteImmediately()
    {
        // Arrange
        var application = new Application { Id = "app1", Name = "App1" };
        _appServiceMock.Setup(x =>
                x.CanDeleteApplicationImmediatelyAsync(It.Is<string>(p => p == application.Id)))
            .ReturnsAsync(true);

        // Act
        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#cannot-delete-immediately-reason"));
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#cannot-delete-immediately-impact"));
    }

    [Fact]
    public void Renders_ExpectedForms_WhenIsPendingDeletion()
    {
        // Arrange
        var application = new Application
        {
            Id = "app1",
            Name = "App1",
            DeleteAt = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Assert
        var actual = cut.Find($"#{DeleteApplicationSection.CancelDeleteFormName}");
        Assert.NotNull(actual);
        Assert.Throws<ElementNotFoundException>(() => cut.Find($"#{DeleteApplicationSection.DeleteFormName}"));
    }

    [Fact]
    public void DoesNotRender_ExpectedForms_WhenIsNotPendingDeletion()
    {
        // Arrange
        var application = new Application
        {
            Id = "app1",
            Name = "App1",
            DeleteAt = null
        };

        // Act
        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find($"#{DeleteApplicationSection.CancelDeleteFormName}"));
        var actual = cut.Find($"#{DeleteApplicationSection.DeleteFormName}");
        Assert.NotNull(actual);
    }
}