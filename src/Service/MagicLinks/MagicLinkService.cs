using Passwordless.Common.Services.Mail;
using Passwordless.Service.MagicLinks.Models;
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

    public async Task SendMagicLink(MagicLinkDTO dto)
    {
        var token = await _fido2Service.CreateSigninToken(new SigninTokenRequest(dto.UserId));

        var link = new Uri(dto.LinkTemplate.Replace("<token>", token));

        await _mailProvider.SendAsync(new MailMessage
        {
            To = [dto.EmailAddress.ToString()],
            From = "do-not-reply@maila.passwordless.dev",
            Subject = "Verify Email Address",
            TextBody = $"Please click the link or copy into your browser of choice to log in: {link}. If you did not request this email, it is safe to ignore.",
            HtmlBody =
                //lang=html
                $"""
                 <!DOCTYPE html>
                 <html lang="en">
                    <head>
                        <title>Sign-In With Link Request</title>
                    </head>
                    <body style="background-color: white;font-family: DM Sans,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Helvetica Neue,sans-serif;font-size: 1rem;">
                        <h3 style="text-align: center;">Hello</h3>
                        <p style="text-align: center;">Please click the button below to log in.</p>
                        <a href="{link}" style="width: 2.5rem; margin-left: auto; margin-right: auto; padding: .75rem 1.5rem; background-color: rgb(18 82 163); border-radius: 999px; color: white; text-align: center; text-decoration: none; display: block; cursor: pointer;">Login</a>
                        <p>Having trouble? Copy this link to your browser: {link}</p>
                        <p style="text-align: center;">If you did not request this email, it is safe to ignore.</p>
                    </body>
                 </html>
                 """
        });
    }
}