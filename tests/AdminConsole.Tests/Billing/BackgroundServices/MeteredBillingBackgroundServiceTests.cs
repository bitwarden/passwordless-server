using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Passwordless.AdminConsole.Billing.BackgroundServices;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Models.Reporting;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Billing.BackgroundServices;

/// <summary>
/// Regression tests for PM-45932. An application without a subscription item, or any other failure while processing
/// one organization, must not stop metered usage from being processed for the remaining organizations.
/// </summary>
public class MeteredBillingBackgroundServiceTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 28, 23, 0, 0, TimeSpan.Zero));
    private readonly Mock<ISharedBillingService> _sharedBillingServiceMock = new();
    private readonly Mock<ICurrentContext> _currentContextMock = new();
    private readonly Mock<IScopedPasswordlessClient> _passwordlessClientMock = new();
    private readonly List<string> _queriedApplications = new();
    private string? _currentApplicationId;

    public MeteredBillingBackgroundServiceTests()
    {
#pragma warning disable CS0618 // The worker is a valid caller of SetApp.
        _currentContextMock
            .Setup(x => x.SetApp(It.IsAny<Application>()))
            .Callback<Application>(application =>
            {
                _currentApplicationId = application.Id;
                _queriedApplications.Add(application.Id);
            });
#pragma warning restore CS0618
    }

    [Fact]
    public async Task DoWorkAsync_ApplicationWithoutSubscriptionItem_IsSkippedAndLaterOrganizationsAreProcessed()
    {
        var earlierOrganization = CreateOrganization(1,
            CreateApplication("freeapp", subscriptionItemId: null),
            CreateApplication("proapp", subscriptionItemId: "si_a"));
        var laterOrganization = CreateOrganization(2,
            CreateApplication("laterapp", subscriptionItemId: "si_b"));
        SetupPayingOrganizations(earlierOrganization, laterOrganization);

        // Only the free application reports usage, which used to reach the dictionary with a null key.
        _passwordlessClientMock
            .Setup(x => x.GetPeriodicCredentialReportsAsync(It.IsAny<PeriodicCredentialReportRequest>()))
            .ReturnsAsync(() => _currentApplicationId == "freeapp"
                ? [new PeriodicCredentialReportResponse(DateOnly.FromDateTime(_timeProvider.GetUtcNow().Date), 5, 5)]
                : []);

        await RunAsync();

        Assert.Equal(["proapp", "laterapp"], _queriedApplications);
    }

    [Fact]
    public async Task DoWorkAsync_OrganizationFails_LaterOrganizationsAreProcessed()
    {
        var failingOrganization = CreateOrganization(1,
            CreateApplication("failingapp", subscriptionItemId: "si_a"));
        var laterOrganization = CreateOrganization(2,
            CreateApplication("laterapp", subscriptionItemId: "si_b"));
        SetupPayingOrganizations(failingOrganization, laterOrganization);

        _passwordlessClientMock
            .Setup(x => x.GetPeriodicCredentialReportsAsync(It.IsAny<PeriodicCredentialReportRequest>()))
            .Returns(() => _currentApplicationId == "failingapp"
                ? throw new HttpRequestException("Simulated failure.")
                : Task.FromResult<IEnumerable<PeriodicCredentialReportResponse>>([]));

        await RunAsync();

        Assert.Equal(["failingapp", "laterapp"], _queriedApplications);
    }

    private void SetupPayingOrganizations(params Models.Organization[] organizations)
    {
        _sharedBillingServiceMock
            .Setup(x => x.GetPayingOrganizationsAsync())
            .ReturnsAsync(organizations);
    }

    private async Task RunAsync()
    {
        var services = new ServiceCollection()
            .AddSingleton<TimeProvider>(_timeProvider)
            .AddSingleton(_sharedBillingServiceMock.Object)
            .AddSingleton(_currentContextMock.Object)
            .AddSingleton(_passwordlessClientMock.Object)
            .BuildServiceProvider();

        var sut = new MeteredBillingBackgroundService(
            services,
            _timeProvider,
            NullLogger<MeteredBillingBackgroundService>.Instance);

        // DoWorkAsync is normally invoked by the base class timer, call it directly to keep the test deterministic.
        var doWorkAsync = typeof(MeteredBillingBackgroundService)
            .GetMethod("DoWorkAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await (Task)doWorkAsync.Invoke(sut, [CancellationToken.None])!;
    }

    private static Models.Organization CreateOrganization(int id, params Application[] applications) => new()
    {
        Id = id,
        Name = $"Organization {id}",
        BillingCustomerId = $"cus_{id}",
        BillingSubscriptionId = $"sub_{id}",
        Applications = applications.ToList()
    };

    private static Application CreateApplication(string id, string? subscriptionItemId) => new()
    {
        Id = id,
        BillingSubscriptionItemId = subscriptionItemId
    };
}