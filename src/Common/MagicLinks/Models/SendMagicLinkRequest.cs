using System.ComponentModel.DataAnnotations;
using Passwordless.Common.MagicLinks.Validation;

namespace Passwordless.Common.MagicLinks.Models;

public class SendMagicLinkRequest
{
    public const string TokenTemplate = "$TOKEN";

    [Required]
    [EmailAddress]
    public string EmailAddress { get; init; }

    [Required]
    [MagicLinkTemplateUrl]
    public string UrlTemplate { get; init; }

    [Required]
    public string UserId { get; init; }

    /// <summary>
    /// Number of seconds the magic link will be valid for.
    /// </summary>
    public int? TimeToLive { get; init; }
}