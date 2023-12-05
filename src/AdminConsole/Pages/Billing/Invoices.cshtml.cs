using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Billing;

public class InvoicesModel : PageModel
{
    private readonly ISharedBillingService _billingService;

    public InvoicesModel(ISharedBillingService billingService)
    {
        _billingService = billingService;
    }

    public IReadOnlyCollection<InvoiceModel> Invoices { get; set; } = new List<InvoiceModel>(0).ToImmutableList();

    public async Task OnGet()
    {
        Invoices = await _billingService.GetInvoicesAsync();
    }
}