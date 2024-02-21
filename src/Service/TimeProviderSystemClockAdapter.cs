using Microsoft.Extensions.Internal;

namespace Passwordless.Service;

/// <summary>
/// Wraps <see cref="TimeProvider"/> to implement <see cref="ISystemClock"/>.
/// </summary>
public class TimeProviderSystemClockAdapter(TimeProvider timeProvider) :
    ISystemClock,
#pragma warning disable CS0618 // Type or member is obsolete
    Microsoft.AspNetCore.Authentication.ISystemClock
#pragma warning restore CS0618 // Type or member is obsolete
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}