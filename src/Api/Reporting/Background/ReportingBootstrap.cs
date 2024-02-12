namespace Passwordless.Api.Reporting.Background;

public static class ReportingBootstrap
{
    public static void AddReportingBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<PeriodicCredentialReportsBackgroundService>();
        services.AddHostedService<ActiveUserReportingBackgroundService>();
    }
}