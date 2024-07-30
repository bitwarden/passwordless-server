using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Models.Apps;
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

    [Fact]
    public void SubmittingDeleteForm_WithIncorrectNameConfirmation_ShowsValidationMessage()
    {
        // Arrange
        var application = new Application { Id = "app1", Name = "App1" };
        _appServiceMock.Setup(x =>
                x.CanDeleteApplicationImmediatelyAsync(It.Is<string>(p => p == application.Id)))
            .ReturnsAsync(true);

        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Act
        cut.Find("form").Submit();

        // Assert
        var actualErrors = cut.Find("ul.validation-errors");
        Assert.Contains(actualErrors.Children, x => x.TextContent == "Name confirmation does not match.");
    }

    [Fact]
    public void SubmittingDeleteForm_WithCorrectNameConfirmation_NavigatesAwayWhenSuccessful()
    {
        // Arrange
        var application = new Application { Id = "app1", Name = "App1" };
        _appServiceMock.Setup(x =>
                x.CanDeleteApplicationImmediatelyAsync(It.Is<string>(p => p == application.Id)))
            .ReturnsAsync(true);

        const string username = "John Doe";
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.Name, username));
        var principal = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.SetupGet(x => x.HttpContext.User).Returns(principal);

        var expectedDeleteResponse = new MarkDeleteApplicationResponse(true, new DateTime(2024, 1, 1), ["johndoe@example.org"]);

        _appServiceMock.Setup(x =>
                x.MarkDeleteApplicationAsync(
                    It.Is<string>(p => p == application.Id),
                    It.Is<string>(p => p == username)))
            .ReturnsAsync(expectedDeleteResponse);

        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("App1");
        cut.Find($"form#{DeleteApplicationSection.DeleteFormName}").Submit();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("ul.validation-errors"));

        _appServiceMock.Verify(x =>
            x.MarkDeleteApplicationAsync(
                It.Is<string>(p => p == application.Id),
                It.Is<string>(p => p == username)), Times.Once);

        var navMan = Services.GetRequiredService<FakeNavigationManager>();
        Assert.Equal("http://localhost/Organization/Overview", navMan.Uri);
    }

    [Fact]
    public void SubmittingCancelDeleteForm_WithCorrectNameConfirmation_NavigatesToSamePageWhenSuccessful()
    {
        // Arrange
        var application = new Application
        {
            Id = "app1",
            Name = "App1",
            DeleteAt = DateTime.UtcNow.AddMonths(1)
        };

        const string username = "John Doe";
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.Name, username));
        var principal = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.SetupGet(x => x.HttpContext.User).Returns(principal);

        var cut = RenderComponent<DeleteApplicationSection>(c =>
            c.Add(p => p.Application, application));

        // Act
        cut.Find($"form#{DeleteApplicationSection.CancelDeleteFormName}").Submit();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("ul.validation-errors"));

        _appServiceMock.Verify(x =>
            x.CancelDeletionForApplicationAsync(It.Is<string>(p => p == application.Id)), Times.Once);

        var navMan = Services.GetRequiredService<FakeNavigationManager>();
        Assert.Equal("http://localhost/", navMan.Uri);
    }
}