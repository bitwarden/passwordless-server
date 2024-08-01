namespace Passwordless.AdminConsole.FeatureManagement;

public static class FeatureFlags
{
    public static class Organization
    {
        private const string Prefix = "Organization";
        public const string AllowDisablingMagicLinks = $"{Prefix}_AllowDisablingMagicLinks";
    }
}