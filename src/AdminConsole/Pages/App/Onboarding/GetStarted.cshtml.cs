using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Pages.App.Onboarding;

public class GetStarted : PageModel
{
    private readonly IApplicationService _applicationService;
    private readonly ICurrentContext _context;

    public Models.Onboarding Onboarding { get; set; }

    public GetStarted(IApplicationService applicationService, ICurrentContext context, IOptionsSnapshot<PasswordlessManagementOptions> passwordlessOptions)
    {
        _applicationService = applicationService;
        _context = context;
        ApiUrl = passwordlessOptions.Value.ApiUrl;
    }

    public string ApiUrl { get; }

    public async Task<IActionResult> OnGet()
    {
        Onboarding = await _applicationService.GetOnboardingAsync(_context.AppId);

        if (Onboarding == null)
        {
            return RedirectToPage("/Error", new { message = "Onboarding not found" });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadApiKeysAsync()
    {
        Onboarding = await _applicationService.GetOnboardingAsync(_context.AppId);
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine($"API Url: {ApiUrl}");
        contentBuilder.AppendLine($"Public ApiKey: {Onboarding.ApiKey}");
        contentBuilder.AppendLine($"Private ApiSecret: {Onboarding.ApiSecret}");

        // Convert the string to a byte array
        byte[] byteArray = Encoding.UTF8.GetBytes(contentBuilder.ToString());
        return File(byteArray, "application/octet-stream",
            $"passwordless-apikeys-{Onboarding.ApplicationId}.txt");
    }
}