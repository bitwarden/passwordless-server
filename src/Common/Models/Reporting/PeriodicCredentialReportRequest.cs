
namespace Passwordless.Common.Models.Reporting;

/// <param name="From">From when to obtain reports (inclusive), null means from the beginning of time.</param>
/// <param name="To">To when to obtain reports (inclusive), null means to the end of time.</param>
public record PeriodicCredentialReportRequest(DateTime? From, DateTime? To);