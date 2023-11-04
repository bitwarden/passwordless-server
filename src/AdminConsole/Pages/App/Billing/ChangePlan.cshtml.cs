using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Billing;

public class ChangePlanModel : PageModel
{
    private readonly IDataService _dataService;
    private readonly StripeOptions _stripeOptions;

    public ChangePlanModel(
        IDataService dataService,
        IOptions<StripeOptions> stripeOptions)
    {
        _dataService = dataService;
        _stripeOptions = stripeOptions.Value;
    }

    public Application Application { get; private set; }

    public ICollection<PlanModel> Plans { get; } = new List<PlanModel>();

    public async Task OnGet(string app)
    {
        var application = await _dataService.GetApplicationAsync(app);
        if (application == null) throw new InvalidOperationException("Application not found.");
        Application = application;

        AddPlan(PlanConstants.Free, _stripeOptions.Plans[PlanConstants.Free]);
        AddPlan(PlanConstants.Pro, _stripeOptions.Plans[PlanConstants.Pro]);
        AddPlan(PlanConstants.Enterprise, _stripeOptions.Plans[PlanConstants.Enterprise]);
    }

    public async Task<IActionResult> OnPost(string appId, string value)
    {
        return RedirectToPage("/billing/manage");
    }

    private void AddPlan(string plan, StripePlanOptions options)
    {
        var isActive = Application!.BillingPlan == plan;

        bool canSubscribe = plan != PlanConstants.Free && Application.BillingPriceId != options.PriceId;

        var model = new PlanModel(
            plan,
            options.PriceId,
            options.Ui.Label,
            options.Ui.Price,
            options.Ui.PriceHint,
            options.Ui.Features.ToImmutableList(),
            isActive,
            canSubscribe);
        Plans.Add(model);
    }

    public record PlanModel(
        string Value,
        string? PriceId,
        string Label,
        string Price,
        string? PriceHint,
        IReadOnlyCollection<string> Features,
        bool IsActive,
        bool CanSubscribe);
}