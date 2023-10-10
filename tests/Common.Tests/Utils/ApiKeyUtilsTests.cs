using Passwordless.Common.Utils;

namespace Passwordless.Common.Tests.Utils;

public class ApiKeyUtilsTests
{
    #region GetAppId
    [Fact]
    public void GetAppId_Returns_AppId_ForAValidApiSecret()
    {
        var apiSecret = "myapp1:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyUtils.GetAppId(apiSecret);

        Assert.Equal("myapp1", actual);
    }

    [Fact]
    public void GetAppId_Throws_ArgumentException_ForAnInvalidApiSecret()
    {
        var apiSecret = "myapp1";

        Assert.Throws<ArgumentException>(() => ApiKeyUtils.GetAppId(apiSecret));
    }

    [Fact]
    public void GetAppId_Returns_AppId_ForAValidApiKey()
    {
        var apiKey = "myapp1:public:2e728aa5986f4ba8b073a5b28a939795";

        var actual = ApiKeyUtils.GetAppId(apiKey);

        Assert.Equal("myapp1", actual);
    }

    [Fact]
    public void GetAppId_Throws_ArgumentException_ForAnInvalidApiKey()
    {
        var apiKey = "myapp1";

        Assert.Throws<ArgumentException>(() => ApiKeyUtils.GetAppId(apiKey));
    }
    #endregion

    #region Validate
    [Fact]
    public void Validate_Returns_True_ForValidApiKey()
    {
        const string hashedApiKey = "4RtmMr0hVknaQAIhaRtPHw==:xR7bg3NVsC80a8GDDhH39g==";
        const string apiKey = "test:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyUtils.Validate(hashedApiKey, apiKey);

        Assert.True(actual);
    }

    [Fact]
    public void Validate_Returns_False_ForInvalidApiKey()
    {
        const string hashedApiKey = "4RtmMr0hVknaQAIhaRtPHw==:xR7bg3NVsC80a8GDDhH39g==";
        const string apiKey = "test:secret:a679563b331846c79c20b114a4f56d01";

        var actual = ApiKeyUtils.Validate(hashedApiKey, apiKey);

        Assert.False(actual);
    }

    [Fact]
    public void Validate_Returns_False_ForBadHash()
    {
        const string hashedApiKey = "4RtmMr0hVknaQAIhaRtPHw==xR7bg3NVsC80a8GDDhH39g==";
        const string apiKey = "test:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyUtils.Validate(hashedApiKey, apiKey);

        Assert.False(actual);
    }

    [Fact]
    public void Validate_Returns_False_ForBadHash2()
    {
        const string hashedApiKey = "4RtmMr0hVknaQAIhaRtPHw=:xR7bg3NVsC80a8GDDhH39g==";
        const string apiKey = "test:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyUtils.Validate(hashedApiKey, apiKey);

        Assert.False(actual);
    }
    #endregion

    #region HashPrivateApiKey
    [Fact]
    public void HashPrivateApiKey_Returns_ExpectedResult()
    {
        const string apiKey = "test:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyUtils.HashPrivateApiKey(apiKey);

        Assert.True(ApiKeyUtils.Validate(actual, apiKey));
    }
    #endregion
}