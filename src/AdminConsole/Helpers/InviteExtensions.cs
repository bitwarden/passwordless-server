using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Helpers;

public static class InviteExtensions
{
    public static bool IsExpired(this Invite invite, TimeProvider timeProvider) => 
        invite.ExpireAt < timeProvider.GetUtcNow().UtcDateTime;
}