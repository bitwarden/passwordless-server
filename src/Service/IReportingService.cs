using Passwordless.Common.Models.Reporting;

namespace Passwordless.Service;

public interface IReportingService
{
    Task<int> UpdatePeriodicCredentialReportsAsync();
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest parameters);
}