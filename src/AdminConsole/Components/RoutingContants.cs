namespace Passwordless.AdminConsole.Components;

public static class RoutingContants
{
    public static class Billing
    {
        public const string Root = "/billing";

        public const string Cancelled = $"{Root}/cancelled";
        public const string Default = $"{Root}/default";
        public const string Invoices = $"{Root}/invoices";
        public const string Manage = $"{Root}/manage";
        public const string Success = $"{Root}/success";
    }
}