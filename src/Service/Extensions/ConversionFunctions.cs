namespace Passwordless.Service.Extensions;

public static class ConversionFunctions
{
    public static TimeSpan? GetNullableTimeSpanFromSeconds(this int? seconds) =>
        seconds.HasValue
            ? TimeSpan.FromSeconds(seconds.Value)
            : null;
}