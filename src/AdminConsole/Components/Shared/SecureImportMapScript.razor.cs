using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Passwordless.AdminConsole.Components.Shared;

public partial class SecureImportMapScript
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Parameter]
    public required Model Value { get; set; }

    public string? Nonce
    {
        get
        {
            if (HttpContextAccessor.HttpContext?.Items == null)
            {
                return null;
            }

            if (HttpContextAccessor.HttpContext!.Items.TryGetValue("csp-nonce", out var nonce))
            {
                return nonce?.ToString();
            }

            return null;
        }
    }

    protected override void OnInitialized()
    {
        foreach (var import in Value.Imports)
        {
            Value.Imports[import.Key] = Version.Get(import.Value);
        }
    }

    public class Model
    {
        public required Dictionary<string, string> Imports { get; set; }
    }
}