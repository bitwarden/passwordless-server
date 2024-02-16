using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Passwordless.Service.MagicLinks.Validation;

namespace Passwordless.Service.MagicLinks.Models;

public class SendMagicLinkRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; init; }

    [Required]
    [MagicLinkTemplateUrl]
    public string UrlTemplate { get; init; }

    [Required]
    public string UserId { get; init; }

    public MagicLinkDTO ToDto() => new()
    {
        UserId = UserId,
        EmailAddress = new MailAddress(EmailAddress),
        LinkTemplate = UrlTemplate
    };
}