namespace Passwordless.Service.Models;

public class PeriodicActiveUserReport : PerTenant
{
    public DateOnly CreatedAt { get; set; }

    public int DailyActiveUsersCount { get; set; }

    public int WeeklyActiveUsersCount { get; set; }

    public int TotalUsersCount { get; set; }

    public virtual AccountMetaInformation Application { get; init; }
}