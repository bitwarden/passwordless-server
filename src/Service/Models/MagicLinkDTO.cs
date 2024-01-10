using System.Net.Mail;

namespace Passwordless.Service.Models;

public class MagicLinkDTO
{
    public string UserId { get; init; }
    public MailAddress UserEmail { get; init; }
    public string UrlTokenTemplate { get; init; }
}