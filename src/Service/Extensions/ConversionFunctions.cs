namespace Passwordless.Service.Extensions;

public static class ConversionFunctions
{
    public static TimeSpan? ToTimeSpanFromSeconds(this int seconds) => TimeSpan.FromSeconds(seconds);
}