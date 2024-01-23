using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

/// <summary>
/// Renders a <see cref="DateTime" /> in the user's local time zone. 
/// </summary>
public partial class LocalDateTime : ComponentBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalDateTime(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Id { get; } = Guid.NewGuid().ToString();

    [Parameter]
    public required DateTime Value { get; set; }

    public string CspNonce => _httpContextAccessor.HttpContext!.Items["csp-nonce"]!.ToString()!;
}