using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.Account;

public partial class Magic : ComponentBase
{
    private bool? _success;

    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Token))
        {
            _success = false;
            return;
        }

        var res = await SignInManager.PasswordlessSignInAsync(Token, true);

        if (res.Succeeded)
        {
            ReturnUrl ??= "/Organization/Overview";
            NavigationManager.NavigateTo(ReturnUrl);
            return;
        }
        _success = false;
    }
}