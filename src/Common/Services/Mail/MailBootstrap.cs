using Passwordless.Common.Services.Mail.Ordered;

namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMailProvider, OrderedMailProvider>();
    }
}