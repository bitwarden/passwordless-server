using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;

namespace Passwordless.AdminConsole.Pages.Organization;

public class Create : PageModel
{
    private readonly IDataService _dataService;

    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly IMailService _mailService;
    private readonly MagicLinkSignInManager<ConsoleAdmin> _magicLinkSignInManager;
    private readonly IEventLogger _eventLogger;

    public CreateModel Form { get; set; }

    public Create(IDataService dataService,
        UserManager<ConsoleAdmin> userManager,
        IMailService mailService,
        MagicLinkSignInManager<ConsoleAdmin> magicLinkSignInManager,
        IEventLogger eventLogger)
    {
        _dataService = dataService;
        _userManager = userManager;
        _mailService = mailService;
        _magicLinkSignInManager = magicLinkSignInManager;
        _eventLogger = eventLogger;
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
        var org = new Models.Organization
        {
            Name = form.OrgName,
            InfoOrgType = form.OrgType,
            InfoUseCase = form.UseCase,
            CreatedAt = DateTime.UtcNow
        };
        await _dataService.CreateOrganizationAsync(org);

        // Create user
        var user = new ConsoleAdmin
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

        _eventLogger.LogCreateOrganizationCreatedEvent(org, user);

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