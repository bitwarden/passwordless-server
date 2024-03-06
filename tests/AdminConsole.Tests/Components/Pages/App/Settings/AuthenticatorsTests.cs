using AutoFixture;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.MDS;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings;

public class AuthenticatorsTests : TestContext
{
    private readonly Mock<ICurrentContext> _currentContextMock = new();
    private readonly Mock<IPasswordlessManagementClient> _passwordlessManagementClientMock = new();
    private readonly Mock<IScopedPasswordlessClient> _passwordlessClientMock = new();
    private readonly Fixture _fixture = new();

    public AuthenticatorsTests()
    {
        this.Services.AddSingleton(_currentContextMock.Object);
        this.Services.AddSingleton(_passwordlessManagementClientMock.Object);
        this.Services.AddSingleton(_passwordlessClientMock.Object);
    }

    [Fact]
    public void AllowList_Renders_SortedAlphabetically()
    {
        // Arrange
        var mds = new List<EntryResponse>
        {
            new (Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154b1"), "Bitwarden Authenticator", new[] { "FIDO_CERTIFIED", "FIDO_CERTIFIED_L1" }, new[] { "basic_full" }, "icon1"),
            new (Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154a9"), "Authenticator #2", new[] { "FIDO_CERTIFIED", "FIDO_CERTIFIED_L1" }, new[] { "basic_full" }, "icon2"),
            new (Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154a8"), "Authenticator #1", new[] { "FIDO_CERTIFIED_L2" }, new[] { "basic_full" }, "icon3")
        };
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(features);
        _passwordlessManagementClientMock.Setup(x => x.GetMetaDataStatementEntriesAsync(It.IsAny<EntriesRequest>()))
            .ReturnsAsync(mds);
        _passwordlessClientMock.Setup(x =>
                x.GetConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p.IsAllowed == true)))
            .ReturnsAsync(new List<ConfiguredAuthenticatorResponse>
            {
                new(Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154b1"), new DateTime(2024, 01, 01)),
                new(Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154a8"), new DateTime(2024, 01, 01)),
            });

        var cut = RenderComponent<Authenticators>();

        // Act

        // Assert
        var firstAllowedAuthenticator = cut.Instance.Allowlist.ElementAt(0);
        Assert.Equal(Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154a8"), firstAllowedAuthenticator.AaGuid);
        Assert.Equal("Authenticator #1", firstAllowedAuthenticator.Name);
        var secondAllowedAuthenticator = cut.Instance.Allowlist.ElementAt(1);
        Assert.Equal(Guid.Parse("cb69481e-8ff7-4039-93ec-0a2729a154b1"), secondAllowedAuthenticator.AaGuid);
        Assert.Equal("Bitwarden Authenticator", secondAllowedAuthenticator.Name);

        var list = cut.Find("div#allowlist");
        var cards = list.ChildNodes;
        cards[0].MarkupMatches("<div diff:ignoreAttributes><div diff:ignoreAttributes><h3 diff:ignoreAttributes>Authenticator #1</h3><p diff:ignoreAttributes>cb69481e-8ff7-4039-93ec-0a2729a154a8</p></div></div>");
        cards[1].MarkupMatches("<div diff:ignoreAttributes><div diff:ignoreAttributes><h3 diff:ignoreAttributes>Bitwarden Authenticator</h3><p diff:ignoreAttributes>cb69481e-8ff7-4039-93ec-0a2729a154b1</p></div></div>");
    }

    [Fact]
    public void AllowList_Renders_Nothing_WhenAllowListEmpty()
    {
        // Arrange
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(features);
        _passwordlessClientMock.Setup(x =>
                x.GetConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p.IsAllowed == true)))
            .ReturnsAsync(new List<ConfiguredAuthenticatorResponse>());

        // Act
        var cut = RenderComponent<Authenticators>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("div#allowlist"));
    }

    [Fact]
    public void Authenticators_RedirectsTo_Settings_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(features);
        _passwordlessClientMock.Setup(x =>
                x.GetConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p.IsAllowed == true)))
            .ReturnsAsync(new List<ConfiguredAuthenticatorResponse>());
        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var baseUriLength = nav.BaseUri.Length;

        // Act
        var cut = RenderComponent<Authenticators>(parameters =>
            parameters.Add(p => p.AppId, "cb69481e-8ff7-4039-93ec-0a2729a154b1"));

        // Assert
        Assert.Equal("app/cb69481e-8ff7-4039-93ec-0a2729a154b1/settings", nav.Uri.Remove(0, baseUriLength));
    }
}