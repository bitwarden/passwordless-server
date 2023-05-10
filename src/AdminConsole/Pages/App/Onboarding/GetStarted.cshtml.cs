using AdminConsole.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdminConsole.Pages.App.Onboarding;

public class GetStarted : PageModel
{
    private readonly ConsoleDbContext _dbContext;
    private readonly ICurrentContext _context;

    public Models.Onboarding Onboarding { get; set; }

    public GetStarted(ConsoleDbContext dbContext, ICurrentContext context)
    {
        _dbContext = dbContext;
        _context = context;
    }

    public async Task<IActionResult> OnGet()
    {
        var appID = _context.Tenant;
        Onboarding = await _dbContext.Applications.Where(a => a.Id == appID).Select(o => o.Onboarding)
            .FirstOrDefaultAsync();

        if (Onboarding == null)
        {
            return RedirectToPage("/Error", new { message = "Onboarding not found" });
        }

        return Page();
    }
}