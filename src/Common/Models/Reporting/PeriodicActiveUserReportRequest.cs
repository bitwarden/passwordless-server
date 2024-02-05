namespace Passwordless.Common.Models.Reporting;

/// <param name="From">From when to obtain reports (inclusive), null means from the beginning of time.</param>
/// <param name="To">To when to obtain reports (inclusive), null means to the end of time.</param>
public record PeriodicActiveUserReportRequest(DateOnly? From, DateOnly? To)
{
    public static PeriodicActiveUserReportRequest Create(DateTime? from, DateTime? to)
    {
        DateOnly? fromValue = from.HasValue ? DateOnly.FromDateTime(from.Value) : null;
        DateOnly? toValue = to.HasValue ? DateOnly.FromDateTime(to.Value) : null;

        return new PeriodicActiveUserReportRequest(fromValue, toValue);
    }
}