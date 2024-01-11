namespace Passwordless.Service.Models;

public class ApplicationMagicLinkLimits : PerTenant
{
    public int Risk { get; set; }
    public int CurrentMonthlySentEmails { get; set; }

    public DateTimeOffset AssessedOn { get; set; }
    public AccountMetaInformation Application { get; set; }
}