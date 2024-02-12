using Passwordless.Common.Models.Reporting;

namespace Passwordless.Service;

public interface IReportingService
{
    Task<int> UpdatePeriodicCredentialReportsAsync();
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest parameters);
    Task<int> UpdatePeriodicActiveUserReportsAsync();
    Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest request);
}