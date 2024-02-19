using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Passwordless.Service.MagicLinks.Validation;
using Passwordless.Service.Models;

namespace Passwordless.Service.MagicLinks.Models;

public class SendMagicLinkRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; init; }

    [Required]
    [MagicLinkTemplateUrlAttribute]
    public string UrlTemplate { get; init; }

    [Required]
    public string UserId { get; init; }

    /// <summary>
    /// Number of seconds the magic link will be valid for.
    /// </summary>
    public int? TimeToLive { get; init; }

    public MagicLinkTokenRequest ToDto() => new(UserId, new MailAddress(EmailAddress), UrlTemplate);
}