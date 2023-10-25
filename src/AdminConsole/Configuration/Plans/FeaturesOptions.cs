namespace Passwordless.AdminConsole.Configuration.Plans;

public class FeaturesOptions
{
    public bool EventLoggingIsEnabled { get; set; }

    public int EventLoggingRetentionPeriod { get; set; }

    public int MaxAdmins { get; set; }

    public int MaxApplications { get; set; }
}