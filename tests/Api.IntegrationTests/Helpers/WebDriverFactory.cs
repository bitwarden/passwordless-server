using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.VirtualAuth;

namespace Passwordless.Api.IntegrationTests.Helpers;

public static class WebDriverFactory
{
    private static WebDriver CreateDriver(string driverUrl)
    {
        var virtualAuth = new VirtualAuthenticatorOptions()
            .SetIsUserVerified(true)
            .SetHasUserVerification(true)
            .SetIsUserConsenting(true)
            .SetTransport(VirtualAuthenticatorOptions.Transport.INTERNAL)
            .SetProtocol(VirtualAuthenticatorOptions.Protocol.CTAP2)
            .SetHasResidentKey(true);

        var options = new ChromeOptions();
        options.AddArguments(
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--headless",
            $"--remote-debugging-port={PortUtilities.FindFreePort()}"
        );

        var driver = new ChromeDriver(options)
        {
            Url = driverUrl
        };

        driver.AddVirtualAuthenticator(virtualAuth);

        return driver;
    }

    public static WebDriver GetDriver(string driverUrl)
    {
        // WebDriver initialization can be flaky, so we retry a few times
        for (int retriesRemaining = 5; ; retriesRemaining--)
        {
            var driver = default(WebDriver?);

            try
            {
                driver = CreateDriver(driverUrl);

                var isDriverReady = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(d =>
                    ((IJavaScriptExecutor)d).ExecuteScript(
                        // lang=javascript
                        "return navigator.credentials !== undefined"
                    ) is true
                );

                if (isDriverReady)
                    return driver;

                driver.Dispose();

                if (retriesRemaining <= 0)
                    throw new TimeoutException("Failed to initialize the web driver.");
            }
            catch (WebDriverTimeoutException)
            {
                driver?.Dispose();

                if (retriesRemaining <= 0)
                    throw;
            }
        }
    }
}