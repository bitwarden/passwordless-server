using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MagicLinks;

public class ApplicationRiskBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationRiskBackgroundService> _logger;

    public ApplicationRiskBackgroundService(IServiceProvider serviceProvider, ILogger<ApplicationRiskBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Application Risk Background Service");

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));

        // need to catch exception thrown when table doesn't exist

        try
        {
            do
            {
                // set application risks based on rules we set.

                // query all existing account metadata information

                // new app is 90 (no existing risk record)

                // app.CreatedDate > 1 day = 80

                // app.CreatedDate > 3 days = 70

                // app.CreatedDate > 7 days = 60

                // app.CreatedDate > 30 days = 50

                // app is paid = 10

                // if app type is manual then ignore (bitwarden employee enforced this)

            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Application Risk Background Service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setting of MagicLinkLimits failed");
        }
    }
}