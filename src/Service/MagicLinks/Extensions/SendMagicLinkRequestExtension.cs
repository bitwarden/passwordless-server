using System.Net.Mail;
using Passwordless.Common.MagicLinks.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.MagicLinks.Extensions;

public static class SendMagicLinkRequestExtension
{
    public static MagicLinkTokenRequest ToDto(this SendMagicLinkRequest request) => new(request.UserId, new MailAddress(request.EmailAddress), request.UrlTemplate, request.TimeToLive);
}