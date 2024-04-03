namespace Passwordless.AdminConsole.Helpers;

public static class TimeSpanExtensions
{
    public static TimeSpan AddSeconds(this TimeSpan timeSpan, int seconds)
        => timeSpan.Add(TimeSpan.FromSeconds(seconds));

    public static TimeSpan AddMinutes(this TimeSpan timeSpan, int minutes)
        => timeSpan.Add(TimeSpan.FromMinutes(minutes));

    public static TimeSpan AddHours(this TimeSpan timeSpan, int hours)
        => timeSpan.Add(TimeSpan.FromHours(hours));

    public static TimeSpan AddDays(this TimeSpan timeSpan, int days)
        => timeSpan.Add(TimeSpan.FromDays(days));
}