using System.Configuration;
using FluentAssertions;
using Passwordless.Common.Services.Mail;

namespace Passwordless.Common.Tests.Services.Mail;

public class PostmarkMailClientFactoryTests
{

    [Fact]
    public void GetClient_GivenEmptyName_WhenThereAreNoClients_ThenDefaultClientShouldBeReturned()
    {
        // Arrange
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new PostmarkClientConfiguration { Name = "Default", ApiKey = "ApiKey", From = "do-not-reply@passwordless.dev" }
        };

        // Act
        var sut = new PostmarkMailClientFactory(configuration);
        var client = sut.GetClient(string.Empty);

        // Assert
        client.Should().NotBeNull();
        client.Name.Should().Be(configuration.DefaultConfiguration.Name);
    }

    [Fact]
    public void GetClient_GivenEmptyName_WhenDefaultClientIsAlreadyCreated_ThenExistingClientShouldBeReturned()
    {
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new PostmarkClientConfiguration { Name = "Default", ApiKey = "ApiKey", From = "do-not-reply@passwordless.dev" }
        };
        var sut = new PostmarkMailClientFactory(configuration);
        var existingClient = sut.GetClient(string.Empty);

        // Act
        var client = sut.GetClient(string.Empty);

        // Assert
        client.Should().NotBeNull();
        client.Should().Be(existingClient);
    }

    [Fact]
    public void GetClient_GivenMagicLinksName_WhenThereAreNoClients_ThenMagicLinksClientShouldBeReturned()
    {
        const string magicLinksName = "magic-links";
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new PostmarkClientConfiguration { Name = "Default", ApiKey = "ApiKey", From = "do-not-reply@passwordless.dev" },
            MessageStreams = new List<PostmarkClientConfiguration>
            {
                new() { Name = magicLinksName, ApiKey = "MagicLinksApiKey", From = "do-not-reply@maila.passwordless.dev" }
            }
        };
        var sut = new PostmarkMailClientFactory(configuration);

        // Act
        var client = sut.GetClient(magicLinksName);

        // Assert
        client.Should().NotBeNull();
        client.Name.Should().Be(magicLinksName);
    }

    [Fact]
    public void GetClient_GivenMagicLinksName_WhenThereIsAnExistingMagicLinksClient_ThenExistingMagicLinksClientShouldBeReturned()
    {
        const string magicLinksName = "magic-links";
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new()
            {
                Name = "Default",
                ApiKey = "ApiKey",
                From = "do-not-reply@passwordless.dev"
            },
            MessageStreams = new List<PostmarkClientConfiguration>
            {
                new()
                {
                    Name = magicLinksName,
                    ApiKey = "MagicLinksApiKey",
                    From = "do-not-reply@maila.passwordless.dev"
                }
            }
        };
        var sut = new PostmarkMailClientFactory(configuration);
        var existingClient = sut.GetClient(magicLinksName);

        // Act
        var client = sut.GetClient(magicLinksName);

        // Assert
        client.Should().NotBeNull();
        client.Should().Be(existingClient);
    }

    [Fact]
    public void GetClient_GivenMagicLinksName_WhenThereIsAnExistingDefaultClient_ThenExistingMagicLinksClientShouldBeReturned()
    {
        const string magicLinksName = "magic-links";
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new()
            {
                Name = "Default",
                ApiKey = "ApiKey",
                From = "do-not-reply@passwordless.dev"
            },
            MessageStreams = new List<PostmarkClientConfiguration>
            {
                new()
                {
                    Name = magicLinksName,
                    ApiKey = "MagicLinksApiKey",
                    From = "do-not-reply@maila.passwordless.dev"
                }
            }
        };
        var sut = new PostmarkMailClientFactory(configuration);
        var defaultClient = sut.GetClient(string.Empty);

        // Act
        var client = sut.GetClient(magicLinksName);

        // Assert
        client.Should().NotBeNull();
        client.Should().NotBe(defaultClient);
    }

    [Fact]
    public void GetClient_GivenNameThatDoesNotExist_WhenNoClients_ThenConfigurationErrorExceptionShouldBeThrown()
    {
        const string misconfiguredName = "misconfigured";
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new PostmarkClientConfiguration { Name = "Default", ApiKey = "ApiKey", From = "do-not-reply@passwordless.dev" },
        };
        var sut = new PostmarkMailClientFactory(configuration);

        // Act
        var action = () => sut.GetClient(misconfiguredName);

        // Assert
        action.Should().Throw<ConfigurationErrorsException>()
            .WithMessage($"The {misconfiguredName} message stream is not properly configured for Postmark.");
    }
}