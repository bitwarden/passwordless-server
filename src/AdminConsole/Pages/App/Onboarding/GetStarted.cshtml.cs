using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Onboarding;

public class GetStarted : PageModel
{
    private readonly IApplicationService _applicationService;
    private readonly ICurrentContext _context;
    private readonly IOptionsSnapshot<PasswordlessOptions> _passwordlessOptions;

    public string ApiUrl => _passwordlessOptions.Value.ApiUrl;

    public Models.Onboarding Onboarding { get; set; }

    public GetStarted(IApplicationService applicationService, ICurrentContext context, IOptionsSnapshot<PasswordlessOptions> passwordlessOptions)
    {
        _applicationService = applicationService;
        _context = context;
        _passwordlessOptions = passwordlessOptions;
    }

    public async Task<IActionResult> OnGet()
    {
        Onboarding = await _applicationService.GetOnboardingAsync(_context.AppId);

        if (Onboarding == null)
        {
            return RedirectToPage("/Error", new { message = "Onboarding not found" });
        }

        return Page();
    }
}