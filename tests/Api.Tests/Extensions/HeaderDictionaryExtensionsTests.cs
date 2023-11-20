using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Passwordless.Api.Extensions;

namespace Passwordless.Api.Tests.Extensions;

public class HeaderDictionaryExtensionsTests
{
    [Fact]
    public void GetApiSecret_GivenHeaderDictionary_WhenApiSecretHeaderExists_ThenTenantSecretValueShouldReturn()
    {
        const string expected = "test:secret:2e728aa5986f4ba8b073a5b28a93979";
        var headers = new HeaderDictionary { { "ApiSecret", expected } };

        var actual = headers.GetApiSecret();

        actual.Should().Be(expected);
    }

    [Fact]
    public void GetApiSecret_GivenEmptyHeaderDictionary_WhenApiSecretIsNotPresent_ThenNullShouldReturn()
    {
        var actual = new HeaderDictionary().GetApiSecret();

        actual.Should().BeNull();
    }

    [Fact]
    public void GetApiSecret_GivenHeaderDictionary_WhenApiSecretIsNotPresent_ThenNullShouldReturn()
    {
        var headers = new HeaderDictionary { { "SomeKey", "SomeValue" } };

        var actual = headers.GetApiSecret();

        actual.Should().BeNull();
    }

    [Fact]
    public void GetPublicApiKey_GivenHeaderDictionary_WhenPublicKeyHeaderExists_ThenTenantSecretValueShouldReturn()
    {
        const string expected = "test:public:2e728aa5986f4ba8b073a5b28a93979";
        var headers = new HeaderDictionary { { "ApiKey", expected } };

        var actual = headers.GetPublicApiKey();

        actual.Should().Be(expected);
    }

    [Fact]
    public void GetPublicApiKey_GivenEmptyHeaderDictionary_WhenPublicKeyIsNotPresent_ThenNullShouldReturn()
    {
        var actual = new HeaderDictionary().GetPublicApiKey();

        actual.Should().BeNull();
    }

    [Fact]
    public void GetPublicApiKey_GivenHeaderDictionary_WhenPublicKeyIsNotPresent_ThenNullShouldReturn()
    {
        var headers = new HeaderDictionary { { "SomeKey", "SomeValue" } };

        var actual = headers.GetPublicApiKey();

        actual.Should().BeNull();
    }
}