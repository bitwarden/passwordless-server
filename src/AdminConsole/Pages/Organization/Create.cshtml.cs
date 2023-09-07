using System.ComponentModel.DataAnnotations;
using AdminConsole.Db;
using AdminConsole.Identity;
using AdminConsole.Services.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.AuditLog.Loggers;
using static Passwordless.AdminConsole.AuditLog.AuditLogEventFunctions;

namespace AdminConsole.Pages.Organization;

public class Create : PageModel
{
    private readonly ConsoleDbContext _context;

    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly IMailService _mailService;
    private readonly MagicLinkSignInManager<ConsoleAdmin> _magicLinkSignInManager;
    private readonly IAuditLogger _auditLogger;

    public CreateModel Form { get; set; }

    public Create(ConsoleDbContext context,
        UserManager<ConsoleAdmin> userManager,
        IMailService mailService,
        MagicLinkSignInManager<ConsoleAdmin> magicLinkSignInManager,
        IAuditLogger auditLogger)
    {
        _context = context;
        _userManager = userManager;
        _mailService = mailService;
        _magicLinkSignInManager = magicLinkSignInManager;
        _auditLogger = auditLogger;
    }

    public IActionResult OnGet()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Organization/Overview");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost(CreateModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check if admin email is already used? (Use UserManager)
        var existingUser = await _userManager.FindByEmailAsync(form.AdminEmail);

        if (existingUser != null)
        {
            await _mailService.SendEmailIsAlreadyInUseAsync(existingUser.Email);
            return RedirectToPage("/Organization/Verify");
        }

        // Create org
        var org = new Models.Organization()
        {
            Name = form.OrgName,
            InfoOrgType = form.OrgType,
            InfoUseCase = form.UseCase,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync(cancellationToken);

        // Create user
        var user = new ConsoleAdmin()
        {
            UserName = form.AdminEmail,
            Email = form.AdminEmail,
            OrganizationId = org.Id,
            Name = form.AdminName
        };

        await _userManager.SetUserNameAsync(user, form.AdminEmail);
        await _userManager.SetEmailAsync(user, form.AdminEmail);
        await _userManager.CreateAsync(user);

        var url = Url.Page("/Account/useronboarding");

        await _magicLinkSignInManager.SendEmailForSignInAsync(user.Email, url);

        _auditLogger.LogEvent(CreateOrganizationCreatedEvent(org, user));

        return RedirectToPage("/Organization/Verify");
    }
}

public record CreateModel
{
    [Required, MaxLength(50)]
    public string OrgName { get; set; }
    public string OrgType { get; set; }
    public string UseCase { get; set; }
    [Required, EmailAddress, MaxLength(50)]
    public string AdminEmail { get; set; }
    [Required, MaxLength(50)]
    public string AdminName { get; set; }
    [Required]
    public bool AcceptsTermsAndPrivacy { get; set; }
}