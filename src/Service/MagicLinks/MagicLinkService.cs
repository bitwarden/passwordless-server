using Passwordless.Common.Services.Mail;
using Passwordless.Service.Models;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService
{
    private readonly IFido2Service _fido2Service;
    private readonly IMailProvider _mailProvider;

    public MagicLinkService(IFido2Service fido2Service, IMailProvider mailProvider)
    {
        _fido2Service = fido2Service;
        _mailProvider = mailProvider;
    }

    public async Task<MagicLinkResult> SendMagicLink(MagicLinkDTO dto)
    {
        // generate verification token
        var token = await _fido2Service.CreateSigninToken(new SigninTokenRequest(dto.UserId));

        // generate the magic link
        var link = new Uri(dto.UrlTokenTemplate.Replace("<token>", token));

        // send magic link
        await _mailProvider.SendAsync(new MailMessage
        {
            To = new[] { dto.UserEmail.ToString() },
            From = "do-not-reply@passwordless.dev",
            Subject = "Magic Link",
            TextBody = $"Click the link to sign in: {link}",
            HtmlBody = $"<a href=\"{link}\">Click here</a> to sign in"
        });

        return new MagicLinkResult();
    }

    public class Result
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MagicLinkResult : Result
    {

    }
}