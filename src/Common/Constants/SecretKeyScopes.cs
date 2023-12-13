using System.ComponentModel;

namespace Passwordless.Common.Constants;

public enum SecretKeyScopes
{
    [Description("token_register")]
    TokenRegister,

    [Description("token_verify")]
    TokenVerify
}