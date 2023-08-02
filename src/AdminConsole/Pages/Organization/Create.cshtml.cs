using System.ComponentModel.DataAnnotations;
using AdminConsole.Db;
using AdminConsole.Identity;
using AdminConsole.Services.Mail;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;

namespace AdminConsole.Pages.Organization;

public class Create : PageModel
{
    private readonly ConsoleDbContext _context;

    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly IMailService _mailService;
    private readonly MagicLinkSignInManager<ConsoleAdmin> _magicLinkSignInManager;
    private readonly IValidator<CreateModel> _validator;

    public CreateModel Form { get; set; }

    public Create(ConsoleDbContext context,
        UserManager<ConsoleAdmin> userManager,
        IMailService mailService, MagicLinkSignInManager<ConsoleAdmin> magicLinkSignInManager,
        IValidator<CreateModel> validator)
    {
        _context = context;
        _userManager = userManager;
        _mailService = mailService;
        _magicLinkSignInManager = magicLinkSignInManager;
        _validator = validator;
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
        var input = form;
        var input2 = Form;

        if (!form.AcceptsTermsAndPrivacy)
        {
            ModelState.AddModelError("AcceptsTermsAndPrivacy", "You must accept the terms and privacy policy to continue.");
        }

        var validationResult = await _validator.ValidateAsync(form, cancellationToken);

        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);

            return Page();
        }

        // Check if admin email is already used? (Use UserManager)
        var existingUser = await _userManager.FindByEmailAsync(input.AdminEmail);

        if (existingUser != null)
        {
            await _mailService.SendEmailIsAlreadyInUseAsync(existingUser.Email);
            return RedirectToPage("/Organization/Verify");
        }

        // Create org
        var org = new Models.Organization()
        {
            Name = input.OrgName,
            InfoOrgType = input.OrgType,
            InfoUseCase = input.UseCase,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync(cancellationToken);

        // Create user
        var user = new ConsoleAdmin()
        {
            UserName = input.AdminEmail,
            Email = input.AdminEmail,
            OrganizationId = org.Id,
            Name = input.AdminName
        };

        await _userManager.SetUserNameAsync(user, input.AdminEmail);
        await _userManager.SetEmailAsync(user, input.AdminEmail);
        await _userManager.CreateAsync(user);

        var url = Url.Page("/Account/useronboarding");

        await _magicLinkSignInManager.SendEmailForSignInAsync(user.Email, url);

        return RedirectToPage("/Organization/Verify");
    }


}

public record CreateModel
{
    public string OrgName { get; set; }
    public string OrgType { get; set; }
    public string UseCase { get; set; }
    public string AdminEmail { get; set; }
    public string AdminName { get; set; }

    [Required]
    public bool AcceptsTermsAndPrivacy { get; set; }
}

public class CreateModelValidator : AbstractValidator<CreateModel>
{
    public CreateModelValidator()
    {
        RuleFor(x => x.OrgName).NotNull().NotEmpty().MaximumLength(50);
        RuleFor(x => x.AdminEmail).NotNull().NotEmpty().EmailAddress().MaximumLength(50);
        RuleFor(x => x.AdminName).NotNull().NotEmpty().MaximumLength(50);
    }
}