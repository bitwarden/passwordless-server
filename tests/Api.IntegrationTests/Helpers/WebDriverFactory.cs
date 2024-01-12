using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.VirtualAuth;

namespace Passwordless.Api.IntegrationTests.Helpers;

public static class WebDriverFactory
{
    public static WebDriver GetWebDriver(string driverUrl)
    {
        for (int i = 0; i < 5; i++)
        {
            ChromeDriver? driver = null;
            try
            {
                (driver, var res) = GetDriver(driverUrl);
                if (res)
                {
                    Console.WriteLine("Driverdebug: Successfully created driver: " + i);
                    return driver;
                }
            }
            catch (WebDriverTimeoutException e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Driverdebug: Waited but need to retry: " + i);

            driver?.Quit();

        }

        throw new InvalidOperationException("Driverdebug: Could not create a chrome driver");
    }

    private static (ChromeDriver driver, bool res) GetDriver(string driverUrl)
    {
        var virtualAuth = new VirtualAuthenticatorOptions()
            .SetIsUserVerified(true)
            .SetHasUserVerification(true)
            .SetIsUserConsenting(true)
            .SetTransport(VirtualAuthenticatorOptions.Transport.INTERNAL)
            .SetProtocol(VirtualAuthenticatorOptions.Protocol.CTAP2)
            .SetHasResidentKey(true);

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--headless", $"--remote-debugging-port={PortUtilities.FindFreePort()}");
        var driver = new ChromeDriver(options);
        driver.Url = driverUrl;
        driver.AddVirtualAuthenticator(virtualAuth);

        WebDriverWait waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        var sw = Stopwatch.StartNew();
        var res = waiter.Until((webDriver =>
        {
            var exists = (webDriver as IJavaScriptExecutor).ExecuteScript("return navigator.credentials != undefined") as bool?;
            return exists == true;
        }));
        sw.Stop();
        Console.WriteLine($"Driverdebug: Waited for {sw.ElapsedMilliseconds}ms for navigator.credentials to be available");
        return (driver, res);
    }
}