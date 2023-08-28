namespace Passwordless.AdminConsole.Features;

public interface IFeaturesContext
{
    Task<bool> IsAuditLoggingEnabled();
}