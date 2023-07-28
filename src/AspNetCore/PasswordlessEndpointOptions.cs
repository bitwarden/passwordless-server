using System.Diagnostics;
using System.Text;

namespace Passwordless.AspNetCore;

/// <summary>
/// 
/// </summary>
[DebuggerDisplay("{DebuggerToString(),nq}")]
public class PasswordlessEndpointOptions
{
    /// <summary>
    /// 
    /// </summary>
    public PasswordlessEndpointOptions(bool enableRegisterEndpoint = false)
    {
        if (enableRegisterEndpoint)
        {
            RegisterPath = "/passwordless-register";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string GroupPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string? RegisterPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? LoginPath { get; set; } = "/passwordless-login";

    /// <summary>
    /// 
    /// </summary>
    public string? AddCredentialPath { get; set; } = "/passwordless-add-credential";

    private string DebuggerToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{nameof(GroupPrefix)} = ");
        sb.Append(GroupPrefix);
        sb.Append(", ");
        sb.Append($"{nameof(RegisterPath)}");
        sb.Append(string.IsNullOrEmpty(RegisterPath) ? "(disabled)" : RegisterPath);
        sb.Append(", ");

        sb.Append($"{nameof(LoginPath)}");
        sb.Append(string.IsNullOrEmpty(LoginPath) ? "(disabled)" : LoginPath);
        sb.Append(", ");

        sb.Append($"{nameof(AddCredentialPath)}");
        sb.Append(string.IsNullOrEmpty(AddCredentialPath) ? "(disabled)" : AddCredentialPath);

        return sb.ToString();
    }
}
