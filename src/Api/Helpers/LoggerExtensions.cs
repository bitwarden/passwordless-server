namespace Passwordless.Api.Helpers;

public static partial class LoggerExtensions
{
    [LoggerMessage(10000, LogLevel.Error, "The API has encountered an error")]
    public static partial void UncaughtException(this ILogger logger, Exception exception);

    [LoggerMessage(10001, LogLevel.Warning, "Uncaught API Exception")]
    public static partial void UncaughtApiException(this ILogger logger, Exception exception);
}