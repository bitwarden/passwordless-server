using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.Reporting;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize);
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest request);
    Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest request);

    /// <summary>
    /// Returns a list of configured authenticators for the current app. If the list is empty, all authenticators are allowed.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IEnumerable<ConfiguredAuthenticatorResponse>> GetConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request);

    /// <summary>
    /// Add specified authenticators to the whitelist/blacklist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request);

    /// <summary>
    /// Remove specified authenticators from the whitelist/blacklist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request);

    Task SetFeaturesAsync(SetFeaturesRequest request);

    Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationsAsync();
    Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationAsync(string purpose);
    Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration);
    Task SaveAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration);
    Task DeleteAuthenticationConfigurationAsync(DeleteAuthenticationConfigurationRequest purpose);
}