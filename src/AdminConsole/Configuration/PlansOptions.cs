using Passwordless.AdminConsole.Configuration.Plans;

namespace Passwordless.AdminConsole.Configuration;

public class PlansOptions : Dictionary<string, Plan>
{
    public const string RootKey = "Plans";
}