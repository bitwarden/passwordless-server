using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.VirtualAuth;

namespace Passwordless.Api.Integration.Tests;

public static class WebDriverFactory
{
    public static WebDriver GetWebDriver(string driverUrl)
    {
        var virtualAuth = new VirtualAuthenticatorOptions()
            .SetIsUserVerified(true)
            .SetHasUserVerification(true)
            .SetIsUserConsenting(true)
            .SetTransport(VirtualAuthenticatorOptions.Transport.INTERNAL)
            .SetProtocol(VirtualAuthenticatorOptions.Protocol.CTAP2)
            .SetHasResidentKey(true);

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--headless");
        var driver = new ChromeDriver(options);
        driver.Url = driverUrl;
        driver.AddVirtualAuthenticator(virtualAuth);

        return driver;
    }
}