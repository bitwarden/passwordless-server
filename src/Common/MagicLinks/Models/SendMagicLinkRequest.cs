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

    [Required(AllowEmptyStrings = false)]
    public string UserId { get; init; }

    /// <summary>
    /// Represents the time to live (TTL) of a magic link in seconds.
    /// </summary>
    /// <remarks>
    /// The TTL is the lifespan of a magic link, i.e., the duration for which the link is valid.
    /// </remarks>
    [Range(1, 604800)]
    public int? TimeToLive { get; init; }
}