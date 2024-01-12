using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
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
        var res = waiter.Until((webDriver  =>
        {
            var exists =  ( webDriver as IJavaScriptExecutor).ExecuteScript("return navigator.credentials != undefined") as bool?;
            return exists == true;
        }));
        sw.Stop();
        Console.WriteLine($"Waited for {sw.ElapsedMilliseconds}ms for navigator.credentials to be available");
        
        // if res is fals, the test will fail
        
        return driver;
    }
}