using System.Net.Mail;
using Passwordless.Service.Extensions;

namespace Passwordless.Service.Models;

/// <summary>
/// Used to generate and send out the magic link email.
/// </summary>
/// <param name="userId">User identifier for the recipient of the magic link.</param>
/// <param name="emailAddress">Email address for the intended user.</param>
/// <param name="linkTemplate">Template used for creating the magic link. Token template string will be swapped with actual token.</param>
/// <param name="timeToLive">Time span the magic link will be valid for. Default lifespan is 15 minutes.</param>
public class MagicLinkTokenRequest(string userId, MailAddress emailAddress, string linkTemplate, TimeSpan? timeToLive) : RequestBase
{
    private static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromMinutes(15);

    public MagicLinkTokenRequest(string userId, MailAddress emailAddress, string linkTemplate, int? timeToLive)
        : this(userId, emailAddress, linkTemplate, timeToLive.GetNullableTimeSpanFromSeconds()) { }

    public string UserId { get; } = userId;
    public MailAddress EmailAddress { get; } = emailAddress;
    public string LinkTemplate { get; } = linkTemplate;
    public TimeSpan TimeToLive { get; } = timeToLive ?? DefaultTimeToLive;
}