using System.ComponentModel;

namespace Passwordless.Common.Constants;

public enum PublicKeyScopes
{
    [Description("register")]
    Register,

    [Description("login")]
    Login
}