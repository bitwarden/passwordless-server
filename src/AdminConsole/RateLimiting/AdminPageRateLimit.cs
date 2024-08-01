using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.RateLimiting;

public static class AdminPageRateLimit
{
    public const string PolicyName = "organizationIdFixedLength";
    private const int PermitLimit = 50;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private const int QueueLimit = 0;

    public static RateLimiterOptions AddAdminPageRateLimitPolicy(this RateLimiterOptions limiter) =>
        limiter.AddPolicy(PolicyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.User.GetOrganizationId().ToString(),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = PermitLimit,
                    Window = Window,
                    QueueLimit = QueueLimit
                }));
}