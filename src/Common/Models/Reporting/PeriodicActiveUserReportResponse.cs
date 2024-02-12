namespace Passwordless.Common.Models.Reporting;

/// <param name="CreatedAt">Day the report was collected.</param>
/// <param name="DailyActiveUsers">Daily active users by definition of having used at least one of their credentials once.</param>
/// <param name="WeeklyActiveUsers">Weekly active users by definition of having used at least one of their credentials once.</param>
/// <param name="TotalUsers">Total amount of users.</param>
public record PeriodicActiveUserReportResponse(DateOnly CreatedAt, int DailyActiveUsers, int WeeklyActiveUsers, int TotalUsers);