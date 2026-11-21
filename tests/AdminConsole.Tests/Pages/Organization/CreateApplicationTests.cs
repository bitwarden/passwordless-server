using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Pages.Organization;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AdminConsole.Tests.DataFactory;
using Passwordless.Common.Models.Apps;
using Xunit;
using StoreOptions = Passwordless.AdminConsole.Billing.Configuration.StoreOptions;

namespace Passwordless.AdminConsole.Tests.Pages.Organization;

/// <summary>
/// Regression tests for PM-45932. The submitted plan must be one the organization is actually offered, otherwise a
/// subscribed organization can create applications without a subscription item.
/// </summary>
public class CreateApplicationTests
{
    private const string FreePlan = "Free";
    private const string ProPlan = "Pro-2023-2";
    private const string EnterprisePlan = "Enterprise";

    private readonly Mock<IApplicationService> _applicationServiceMock = new();
    private readonly Mock<IDataService> _dataServiceMock = new();
    private readonly Mock<IPasswordlessManagementClient> _managementClientMock = new();
    private readonly Mock<ISharedBillingService> _billingServiceMock = new();

    public CreateApplicationTests()
    {
        _dataServiceMock.Setup(x => x.AllowedToCreateApplicationAsync()).ReturnsAsync(true);
        _managementClientMock
            .Setup(x => x.CreateApplicationAsync(It.IsAny<string>(), It.IsAny<CreateAppDto>()))
            .ReturnsAsync(new CreateAppResultDto
            {
                ApiKey1 = "app:public:key1",
                ApiKey2 = "app:public:key2",
                ApiSecret1 = "app:secret:secret1",
                ApiSecret2 = "app:secret:secret2"
            });
        _billingServiceMock
            .Setup(x => x.CreateSubscriptionItem(It.IsAny<Models.Organization>(), It.IsAny<string>()))
            .ReturnsAsync(("si_new", "price_new"));
    }

    [Fact]
    public async Task OnPost_SubscribedOrganization_FreePlan_IsRejected()
    {
        SetupOrganization(hasSubscription: true);
        var sut = CreateSut();

        var result = await sut.OnPost(CreateForm(FreePlan));

        AssertRejected(sut, result);
    }

    [Fact]
    public async Task OnPost_SubscribedOrganization_UnknownPlan_IsRejected()
    {
        SetupOrganization(hasSubscription: true);
        var sut = CreateSut();

        var result = await sut.OnPost(CreateForm("NotARealPlan"));

        AssertRejected(sut, result);
    }

    [Fact]
    public async Task OnPost_UnsubscribedOrganization_PaidPlan_IsRejected()
    {
        SetupOrganization(hasSubscription: false);
        var sut = CreateSut();

        var result = await sut.OnPost(CreateForm(ProPlan));

        AssertRejected(sut, result);
    }

    [Theory]
    [InlineData(ProPlan)]
    [InlineData(EnterprisePlan)]
    public async Task OnPost_SubscribedOrganization_OfferedPlan_CreatesApplicationWithSubscriptionItem(string plan)
    {
        SetupOrganization(hasSubscription: true);
        var sut = CreateSut();

        var result = await sut.OnPost(CreateForm(plan));

        Assert.IsType<RedirectResult>(result);
        _billingServiceMock.Verify(x => x.CreateSubscriptionItem(It.IsAny<Models.Organization>(), plan), Times.Once);
        _applicationServiceMock.Verify(x => x.CreateApplicationAsync(It.Is<Application>(a =>
            a.BillingPlan == plan && a.BillingSubscriptionItemId == "si_new")), Times.Once);
    }

    [Fact]
    public async Task OnPost_UnsubscribedOrganization_FreePlan_CreatesApplicationWithoutSubscriptionItem()
    {
        SetupOrganization(hasSubscription: false);
        var sut = CreateSut();

        var result = await sut.OnPost(CreateForm(FreePlan));

        Assert.IsType<RedirectResult>(result);
        _billingServiceMock.Verify(x => x.CreateSubscriptionItem(It.IsAny<Models.Organization>(), It.IsAny<string>()), Times.Never);
        _applicationServiceMock.Verify(x => x.CreateApplicationAsync(It.Is<Application>(a =>
            a.BillingPlan == FreePlan && a.BillingSubscriptionItemId == null)), Times.Once);
    }

    private void AssertRejected(CreateApplicationModel sut, IActionResult result)
    {
        Assert.IsType<PageResult>(result);
        Assert.True(sut.ModelState.ContainsKey("Plan"));
        _billingServiceMock.Verify(x => x.CreateSubscriptionItem(It.IsAny<Models.Organization>(), It.IsAny<string>()), Times.Never);
        _managementClientMock.Verify(x => x.CreateApplicationAsync(It.IsAny<string>(), It.IsAny<CreateAppDto>()), Times.Never);
        _applicationServiceMock.Verify(x => x.CreateApplicationAsync(It.IsAny<Application>()), Times.Never);
    }

    private void SetupOrganization(bool hasSubscription)
    {
        _dataServiceMock
            .Setup(x => x.GetOrganizationWithDataAsync())
            .ReturnsAsync(new Models.Organization
            {
                Id = 1,
                Name = "Organization",
                BillingCustomerId = hasSubscription ? "cus_1" : null,
                BillingSubscriptionId = hasSubscription ? "sub_1" : null,
                Applications = new List<Application>(),
                MaxApplications = hasSubscription ? 1000 : 1
            });
    }

    private static CreateApplicationModel.CreateApplicationForm CreateForm(string plan) => new()
    {
        Id = "newapp",
        Name = "New app",
        Description = "A new application",
        Plan = plan
    };

    private CreateApplicationModel CreateSut()
    {
        var passwordlessOptions = new Mock<IOptionsSnapshot<PasswordlessOptions>>();
        passwordlessOptions.Setup(x => x.Value).Returns(new PasswordlessOptions
        {
            ApiUrl = "https://v4.passwordless.dev",
            ApiSecret = "adminconsole:secret:secret"
        });

        var billingOptions = new Mock<IOptionsSnapshot<BillingOptions>>();
        billingOptions.Setup(x => x.Value).Returns(new BillingOptions
        {
            Store = new StoreOptions { Free = FreePlan, Pro = ProPlan, Enterprise = EnterprisePlan },
            Plans = new Dictionary<string, BillingPlanOptions>
            {
                [FreePlan] = CreatePlan("Trial"),
                [ProPlan] = CreatePlan("Pro"),
                [EnterprisePlan] = CreatePlan("Enterprise")
            }
        });

        var signInManager = new Mock<SignInManager<ConsoleAdmin>>(
            new FakeUserManager(),
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ConsoleAdmin>>().Object,
            null!, null!, null!, null!);

        var sut = new CreateApplicationModel(
            passwordlessOptions.Object,
            signInManager.Object,
            _applicationServiceMock.Object,
            _dataServiceMock.Object,
            _managementClientMock.Object,
            billingOptions.Object,
            _billingServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(CustomClaimTypes.OrgId, "1")
            ], "Test"))
        };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new PageActionDescriptor(),
            new ModelStateDictionary());
        sut.PageContext = new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };

        return sut;
    }

    private static BillingPlanOptions CreatePlan(string label) => new()
    {
        Ui = new UiOptions { Label = label },
        Features = new FeaturesOptions()
    };
}