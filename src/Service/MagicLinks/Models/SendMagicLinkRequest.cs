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
    [MagicLinkTemplateUrlAttribute]
    public string UrlTemplate { get; init; }

    [Required]
    public string UserId { get; init; }
}

public static class SendMagicLinkRequestExtensions
{
    public static MagicLinkDTO ToDto(this SendMagicLinkRequest request) =>
        new()
        {
            UserId = request.UserId,
            EmailAddress = new MailAddress(request.EmailAddress),
            LinkTemplate = request.UrlTemplate
        };
}