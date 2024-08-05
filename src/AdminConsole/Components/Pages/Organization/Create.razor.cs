using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Create : ComponentBase
{
    public const string CreateFormName = "create-organization-form";

    [SupplyParameterFromForm(FormName = CreateFormName)]
    public CreateModel? Form { get; set; }

    public EditContext? FormEditContext { get; set; }

    public ValidationMessageStore? FormValidationMessageStore { get; set; }

    protected override void OnInitialized()
    {
        if (HttpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
        {
            NavigationManager.NavigateTo("/organization/overview");
        }

        Form ??= new CreateModel();
        FormEditContext = new EditContext(Form);
        FormValidationMessageStore = new ValidationMessageStore(FormEditContext);
    }

    private async Task OnCreateValidSubmitAsync()
    {
        if (Form == null)
        {
            throw new ArgumentNullException(nameof(Form));
        }

        if (!Form.AcceptsTermsAndPrivacy)
        {
            FormValidationMessageStore!.Add(() => Form.AcceptsTermsAndPrivacy, "You must accept the terms and privacy policy to continue.");
            FormEditContext!.NotifyValidationStateChanged();
            Form.AcceptsTermsAndPrivacy = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(Form.Purpose) || Form.UsePasskeys)
        {
            await Task.Delay(Random.Shared.Next(100, 300));
            Logger.LogInformation("Hidden field submitted from Create. Form: {@form}", Form);
            NavigationManager.NavigateTo("/organization/verify");
            return;
        }

        // Check if admin email is already used? (Use UserManager)
        var existingUser = await UserManager.FindByEmailAsync(Form.AdminEmail);

        if (existingUser != null)
        {
            Logger.LogInformation("Duplicate user submission from Create. Form: {@form}", Form);
            NavigationManager.NavigateTo("/organization/verify");
            return;
        }

        // Create org
        var org = new Models.Organization
        {
            Name = Form.Name,
            InfoOrgType = Form.Type!.Value,
            InfoUseCase = Form.UseCase!.Value,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime
        };
        await DataService.CreateOrganizationAsync(org);

        // Create user
        var user = new ConsoleAdmin
        {
            UserName = Form.AdminEmail,
            Email = Form.AdminEmail,
            OrganizationId = org.Id,
            Name = Form.AdminName
        };

        await UserManager.SetUserNameAsync(user, Form.AdminEmail);
        await UserManager.SetEmailAsync(user, Form.AdminEmail);
        await UserManager.CreateAsync(user);

        const string url = "/account/useronboarding";

        await MagicLinkSignInManager.SendEmailForSignInAsync(user.Email, url);

        EventLogger.LogCreateOrganizationCreatedEvent(org, user);

        NavigationManager.NavigateTo("/organization/verify");
    }

    public record CreateModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Organization name is required")]
        [MaxLength(50, ErrorMessage = "Organization name must be 50 characters or less")]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Organization name can only contain letters, numbers, and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Organization type is required")]
        public OrganizationType? Type { get; set; }

        [Required(ErrorMessage = "Use case is required")]
        public UseCaseType? UseCase { get; set; }

        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Admin email is not a valid email address")]
        [MaxLength(50, ErrorMessage = "Admin email must be 50 characters or less")]
        public string AdminEmail { get; set; }

        [Required(ErrorMessage = "Admin name is required")]
        [MaxLength(50, ErrorMessage = "Admin name must be 50 characters or less")]
        public string AdminName { get; set; }

        public bool AcceptsTermsAndPrivacy { get; set; }

        public string? Purpose { get; set; }

        public bool UsePasskeys { get; set; }
    }
}