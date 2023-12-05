namespace Passwordless.Service.Models;

public class EmailAboutAccountDeletion
{
    public required string CancelLink { get; set; }
    public required string[] Emails { get; set; }
    public required string AccountName { get; set; }
    public required string Message { get; set; }
}