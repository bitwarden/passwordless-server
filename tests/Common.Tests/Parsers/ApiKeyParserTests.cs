using Passwordless.Common.Parsers;

namespace Passwordless.Common.Tests.Parsers;

public class ApiKeyParserTests
{
    [Fact]
    public void GetAppId_Returns_AppId_ForAValidApiSecret()
    {
        var apiSecret = "myapp1:secret:a679563b331846c79c20b114a4f56d02";

        var actual = ApiKeyParser.GetAppId(apiSecret);

        Assert.Equal("myapp1", actual);
    }

    [Fact]
    public void GetAppId_Throws_ArgumentException_ForAnInvalidApiSecret()
    {
        var apiSecret = "myapp1";

        Assert.Throws<ArgumentException>(() => ApiKeyParser.GetAppId(apiSecret));
    }

    [Fact]
    public void GetAppId_Returns_AppId_ForAValidApiKey()
    {
        var apiKey = "myapp1:public:2e728aa5986f4ba8b073a5b28a939795";

        var actual = ApiKeyParser.GetAppId(apiKey);

        Assert.Equal("myapp1", actual);
    }

    [Fact]
    public void GetAppId_Throws_ArgumentException_ForAnInvalidApiKey()
    {
        var apiKey = "myapp1";

        Assert.Throws<ArgumentException>(() => ApiKeyParser.GetAppId(apiKey));
    }
}