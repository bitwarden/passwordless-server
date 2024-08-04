namespace Passwordless.AdminConsole.Components;

public static class RoutingContants
{
    public const string Initialize = "/initialize";
    public const string SignUp = "/signup";

    public static class Account
    {
        public const string Root = "/account";

        public const string AccessDenied = $"{Root}/accessdenied";
        public const string Logout = $"{Root}/logout";
    }

    public static class Billing
    {
        public const string Root = "/billing";

        public const string Cancelled = $"{Root}/cancelled";
        public const string Default = $"{Root}/default";
        public const string Invoices = $"{Root}/invoices";
        public const string Manage = $"{Root}/manage";
        public const string Success = $"{Root}/success";
    }

    public static class Organization
    {
        public const string Root = "/organization";

        public const string Create = $"{Root}/create";
        public const string Admins = $"{Root}/admins";
        public const string Log = $"{Root}/log";
        public const string Settings = $"{Root}/settings";
    }
}