namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Mail:Postmark").Exists())
        {
            builder.Services.AddSingleton<IMailProvider, PostmarkMailProvider>();
        }
        else if (builder.Configuration.GetSection("Mail:Smtp").Exists())
        {
            builder.Services.AddSingleton<IMailProvider, MailKitSmtpMailProvider>();
        }
        else
        {
            builder.Services.AddOptions<FileMailProviderConfiguration>().BindConfiguration("Mail:File");
            builder.Services.AddSingleton<IMailProvider, FileMailProvider>();
        }
    }
}