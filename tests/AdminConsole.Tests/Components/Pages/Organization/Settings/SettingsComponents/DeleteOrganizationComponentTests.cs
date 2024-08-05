using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Components.Pages.Organization.SettingsComponents;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Tests.Factory;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.Organization.Settings.SettingsComponents;

public class DeleteOrganizationComponentTests : TestContext
{
    private readonly Mock<ISharedBillingService> _billingServiceMock = new();
    private readonly Mock<IDataService> _dataServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILogger<DeleteOrganizationComponent>> _loggerMock = new();
    private readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<SignInManager<ConsoleAdmin>> _signInManagerMock;
    private readonly Mock<TimeProvider> _timeProviderMock;

    public DeleteOrganizationComponentTests()
    {
        Services.AddSingleton(_billingServiceMock.Object);
        Services.AddSingleton(_dataServiceMock.Object);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_mailServiceMock.Object);

        Mock<UserManager<ConsoleAdmin>> userManagerMock = new(
            Mock.Of<IUserStore<ConsoleAdmin>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        _signInManagerMock = new Mock<SignInManager<ConsoleAdmin>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ConsoleAdmin>>(),
            null,
            null,
            null,
            null);

        Services.AddSingleton(userManagerMock.Object);
        Services.AddSingleton(_signInManagerMock.Object);

        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2050, 1, 1), TimeSpan.Zero));
        Services.AddSingleton(_timeProviderMock.Object);
    }

    [Fact]
    public void Renders_DeleteForm_WhenOrganizationHasNoApplications()
    {
        // Arrange

        // Act
        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        // Assert
        var actual = cut.Find($"#{DeleteOrganizationComponent.DeleteFormName}");
        Assert.NotNull(actual);
    }

    [Fact]
    public void DoesNotRender_DeleteForm_WhenOrganizationHasApplications()
    {
        // Arrange

        // Act
        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 1)
                .Add(p => p.Name, "Bitwarden"));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find($"#{DeleteOrganizationComponent.DeleteFormName}"));
        Assert.Contains("Your organization 'Bitwarden' has 1 application(s) that are active or pending deletion.",
            cut.Markup);
    }

    [Fact]
    public void SubmittingDeleteForm_WithoutNameConfirmation_ShowsValidationMessage()
    {
        // Arrange
        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        // Act
        cut.Find("form").Submit();

        // Assert
        var actualErrors = cut.Find("ul.validation-errors");
        Assert.Contains(actualErrors.Children, x => x.TextContent == "Please confirm the organization name.");
    }

    [Fact]
    public void SubmittingDeleteForm_WithNameConfirmationTooManyCharacters_ShowsValidationMessage()
    {
        // Arrange
        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change(new string('*', 51));
        cut.Find("form").Submit();

        // Assert
        var actualErrors = cut.Find("ul.validation-errors");
        Assert.Contains(actualErrors.Children,
            x => x.TextContent == "The organization name must be at most 50 characters.");
    }

    [Fact]
    public void SubmittingDeleteForm_WithNameConfirmationNotMatching_ShowsValidationMessage()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitewarden");
        cut.Find("form").Submit();

        // Assert
        var actualErrors = cut.Find("ul.validation-errors");
        Assert.Contains(actualErrors.Children, x => x.TextContent == "Entered organization name does not match.");
    }

    [Fact]
    public void SubmittingDeleteForm_WithNameConfirmationMatching_SendsAnEmail()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var organization = new Models.Organization
        {
            Name = "Bitwarden",
            Admins = new List<ConsoleAdmin>
            {
                new() { Name = "John Doe", Email = "johndoe@example.com" }
            }
        };
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(organization);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitwarden");
        cut.Find("form").Submit();

        // Assert
        _mailServiceMock.Verify(x => x.SendOrganizationDeletedAsync(
            It.Is<string>(p => p == "Bitwarden"),
            It.Is<List<string>>(p => p.All(i => i == "johndoe@example.com")),
            It.Is<string>(p => p == "John Doe"),
            It.Is<DateTime>(p => p == _timeProviderMock.Object.GetUtcNow().UtcDateTime)), Times.Once);
    }

    [Fact]
    public void SubmittingDeleteForm_CancelsActiveSubscriptions()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var organization = new Models.Organization
        {
            Name = "Bitwarden",
            Admins = new List<ConsoleAdmin>
            {
                new() { Name = "John Doe", Email = "johndoe@example.com" }
            },
            BillingSubscriptionId = "sub_123"
        };
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(organization);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        _billingServiceMock.Setup(x =>
                x.CancelSubscriptionAsync(It.Is<string>(p => p == "sub_123")))
            .ReturnsAsync(true);

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitwarden");
        cut.Find("form").Submit();

        // Assert
        _billingServiceMock.Verify(x =>
            x.CancelSubscriptionAsync(It.Is<string>(p => p == "sub_123")), Times.Once);
    }

    [Fact]
    public void SubmittingDeleteForm_WithoutSubscriptions_DoesNotAttemptToCancelSubscriptions()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var organization = new Models.Organization
        {
            Name = "Bitwarden",
            Admins = new List<ConsoleAdmin>
            {
                new() { Name = "John Doe", Email = "johndoe@example.com" }
            },
            BillingSubscriptionId = null
        };
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(organization);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        _billingServiceMock.Setup(x =>
                x.CancelSubscriptionAsync(It.Is<string>(p => p == "sub_123")))
            .ReturnsAsync(true);

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitwarden");
        cut.Find("form").Submit();

        // Assert
        _billingServiceMock.Verify(x =>
            x.CancelSubscriptionAsync(It.Is<string>(p => p == "sub_123")), Times.Never);
    }

    [Fact]
    public void SubmittingDeleteForm_WhenFailingToDeleteOrganization_DoesNotSignOutTheLoggedInAdmin()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var organization = new Models.Organization
        {
            Name = "Bitwarden",
            Admins = new List<ConsoleAdmin>
            {
                new() { Name = "John Doe", Email = "johndoe@example.com" }
            },
            BillingSubscriptionId = null
        };
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(organization);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        _dataServiceMock.Setup(x => x.DeleteOrganizationAsync()).ReturnsAsync(true);

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitwarden");
        cut.Find("form").Submit();

        // Assert
        _dataServiceMock.Verify(x => x.DeleteOrganizationAsync(), Times.Once);
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [Fact]
    public void SubmittingDeleteForm_WhenSuccessfullyDeletingOrganization_SignsOutTheLoggedInAdmin()
    {
        // Arrange
        var principal = ClaimsPrincipalFactory.CreateJohnDoe();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext!.User).Returns(principal);

        var organization = new Models.Organization
        {
            Name = "Bitwarden",
            Admins = new List<ConsoleAdmin>
            {
                new() { Name = "John Doe", Email = "johndoe@example.com" }
            },
            BillingSubscriptionId = null
        };
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(organization);

        var cut = RenderComponent<DeleteOrganizationComponent>(c =>
            c.Add(p => p.ApplicationsCount, 0)
                .Add(p => p.Name, "Bitwarden"));

        _dataServiceMock.Setup(x => x.DeleteOrganizationAsync()).ReturnsAsync(false);

        // Act
        cut.Find("input[name='DeleteForm.NameConfirmation']").Change("Bitwarden");
        cut.Find("form").Submit();

        // Assert
        _dataServiceMock.Verify(x => x.DeleteOrganizationAsync(), Times.Once);
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Never);
    }
}