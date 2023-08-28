namespace Passwordless.AdminConsole.Features;

public class FeaturesContext : IFeaturesContext
{

    public FeaturesContext()
    {
    }

    public async Task<bool> IsAuditLoggingEnabled() => true;
}