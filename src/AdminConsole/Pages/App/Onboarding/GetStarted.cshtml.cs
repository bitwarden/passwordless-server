using AdminConsole.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole;
using Passwordless.Net;

namespace AdminConsole.Pages.App.Onboarding;

public class GetStarted : PageModel
{
    private readonly ConsoleDbContext _dbContext;
    private readonly ICurrentContext _context;
    private readonly IOptionsSnapshot<PasswordlessOptions> _passwordlessOptions;

    public string ApiUrl => _passwordlessOptions.Value.ApiUrl;

    public Models.Onboarding Onboarding { get; set; }

    public GetStarted(ConsoleDbContext dbContext, ICurrentContext context, IOptionsSnapshot<PasswordlessOptions> passwordlessOptions)
    {
        _dbContext = dbContext;
        _context = context;
        _passwordlessOptions = passwordlessOptions;
    }

    public async Task<IActionResult> OnGet()
    {
        var appID = _context.AppId;

        Onboarding = await _dbContext.Applications.Where(a => a.Id == appID).Select(o => o.Onboarding)
            .FirstOrDefaultAsync();

        if (Onboarding == null)
        {
            return RedirectToPage("/Error", new { message = "Onboarding not found" });
        }

        return Page();
    }
}