using System.Reflection;
using System.Text;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings;
using Passwordless.AdminConsole.Components.Shared.Forms;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings;

public class SecretKeyCreatedTests : BunitContext
{
    private const string AppId = "myapp";
    private const string ApiKey = "myapp:secret:fake-key-for-tests";

    private readonly Mock<ICreatedSecretKeyStore> _createdSecretKeyStoreMock = new();

    public SecretKeyCreatedTests()
    {
        Services.AddSingleton(_createdSecretKeyStoreMock.Object);

        ComponentFactories.AddStub<CopyInputField>();
    }

    [Fact]
    public void Render_ShowsTheStashedKey()
    {
        // Arrange
        _createdSecretKeyStoreMock.Setup(x => x.Take(AppId)).Returns(ApiKey);

        // Act
        var cut = Render<SecretKeyCreated>(p => p.Add(x => x.AppId, AppId));

        // Assert
        var copyInputField = cut.FindComponent<Stub<CopyInputField>>();
        Assert.Equal(ApiKey, copyInputField.Instance.Parameters.Get(x => x.Value));
    }

    [Fact]
    public void Render_RedirectsToSettings_WhenNoKeyWasStashed()
    {
        // Arrange
        _createdSecretKeyStoreMock.Setup(x => x.Take(AppId)).Returns((string?)null);

        // Act
        var cut = Render<SecretKeyCreated>(p => p.Add(x => x.AppId, AppId));

        // Assert
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        Assert.Equal($"/app/{AppId}/settings", new Uri(navigationManager.Uri).AbsolutePath);
        Assert.Equal(string.Empty, cut.Instance.ApiKey);
    }

    [Fact]
    public void Render_IgnoresKeyMaterialInTheQueryString()
    {
        // Arrange
        var spoofed = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{AppId}:secret:attacker-chosen"));
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo($"/app/{AppId}/settings/secret-key-created?EncodedApiKey={spoofed}");
        _createdSecretKeyStoreMock.Setup(x => x.Take(AppId)).Returns((string?)null);

        // Act
        var cut = Render<SecretKeyCreated>(p => p.Add(x => x.AppId, AppId));

        // Assert
        Assert.Equal($"/app/{AppId}/settings", new Uri(navigationManager.Uri).AbsolutePath);
        Assert.DoesNotContain("attacker-chosen", cut.Instance.ApiKey);
        Assert.DoesNotContain("attacker-chosen", cut.Markup);
    }

    [Fact]
    public void Render_TakesTheKeyForTheRoutedApplicationOnly()
    {
        // Arrange
        _createdSecretKeyStoreMock.Setup(x => x.Take(AppId)).Returns(ApiKey);

        // Act
        Render<SecretKeyCreated>(p => p.Add(x => x.AppId, AppId));

        // Assert
        _createdSecretKeyStoreMock.Verify(x => x.Take(AppId), Times.Once);
        _createdSecretKeyStoreMock.Verify(x => x.Take(It.Is<string>(id => id != AppId)), Times.Never);
    }

    /// <summary>
    /// Guards against any Admin Console component reading key material from the URL again.
    /// </summary>
    [Fact]
    public void NoComponent_ReadsKeyMaterialFromTheQueryString()
    {
        var assembly = Assembly.GetAssembly(typeof(AdminConsole.Components.App))!;

        var offenders = assembly.GetTypes()
            .Where(type => typeof(ComponentBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(property => (Type: type, Property: property, Attribute: property.GetCustomAttribute<SupplyParameterFromQueryAttribute>())))
            .Where(x => x.Attribute != null)
            .Where(x => IsKeyMaterialName(x.Attribute!.Name ?? x.Property.Name))
            .Select(x => $"{x.Type.Name}.{x.Property.Name}")
            .ToList();

        Assert.Empty(offenders);
    }

    private static bool IsKeyMaterialName(string name) =>
        name.Contains("ApiKey", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Secret", StringComparison.OrdinalIgnoreCase);
}