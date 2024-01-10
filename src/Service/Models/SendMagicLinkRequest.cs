using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Passwordless.Service.Validation;

namespace Passwordless.Service.Models;

public class SendMagicLinkRequest
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; init; }
    
    [Required]
    [MagicLinkTemplateUrl]
    public string MagicLinkUrl { get; init; }
    
    [Required]
    public string UserId { get; init; }
}

public static class SendMagicLinkRequestExtensions
{
    public static MagicLinkDTO ToDto(this SendMagicLinkRequest request) =>
        new()
        {
            UserId = request.UserId,
            UserEmail = new MailAddress(request.UserEmail),
            UrlTokenTemplate = request.MagicLinkUrl 
        };
}