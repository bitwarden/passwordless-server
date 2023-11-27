using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Stripe;

namespace Passwordless.AdminConsole.Pages.Billing;

public class InvoicesModel : PageModel
{
    private readonly IDataService _dataService;

    public InvoicesModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public IReadOnlyCollection<InvoiceModel> Invoices { get; set; } = new List<InvoiceModel>(0);

    public async Task OnGet()
    {
        var organization = await _dataService.GetOrganizationAsync();
        if (!string.IsNullOrEmpty(organization.BillingCustomerId))
        {
            var invoiceService = new InvoiceService();
            var listRequest = new InvoiceListOptions();
            listRequest.Customer = organization.BillingCustomerId;
            listRequest.Limit = 100;
            var invoicesResult = await invoiceService.ListAsync(listRequest);
            if (invoicesResult?.Data == null)
            {
                Invoices = new List<InvoiceModel>(0);
                return;
            }
            Invoices = invoicesResult.Data
                .Where(x => x.InvoicePdf != null)
                .Select(x => new InvoiceModel
                {
                    Number = x.Number,
                    Date = x.Created,
                    Amount = $"{(x.Total / 100.0M):N2} {x.Currency.ToUpperInvariant()}",
                    Pdf = x.InvoicePdf,
                    Paid = x.Paid
                }).ToImmutableList();
        }
    }

    public class InvoiceModel
    {
        /// <summary>
        /// The invoice number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The invoice date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The download link for the invoice
        /// </summary>
        public string Pdf { get; set; }

        public string Amount { get; set; }

        public bool Paid { get; set; }
    }
}