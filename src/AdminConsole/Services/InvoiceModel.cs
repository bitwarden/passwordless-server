namespace Passwordless.AdminConsole.Services;

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