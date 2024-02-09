using Microsoft.Extensions.Internal;

namespace Passwordless.Api.IntegrationTests.Helpers;

public class TimeProviderSystemClock(TimeProvider timeProvider) : ISystemClock
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}