using System.Net.Mail;

namespace Passwordless.Service.MagicLinks.Models;

public class MagicLinkDTO
{
    public string UserId { get; init; }
    public MailAddress EmailAddress { get; init; }
    public string LinkTemplate { get; init; }
}