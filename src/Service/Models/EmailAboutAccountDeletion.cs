namespace Passwordless.Service.Models;

public class EmailAboutAccountDeletion
{
    public string CancelLink { get; set; } = "";
    public string[] Emails { get; set; } = Array.Empty<string>();
    public string AccountName { get; set; } = "";
    public string Message { get; set; } = "";
}