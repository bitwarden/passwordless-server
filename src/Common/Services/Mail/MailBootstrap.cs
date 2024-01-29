namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Mail:Postmark").Exists())
        {
            var configurationSection = builder.Configuration.GetSection("Mail:PostMark");

            builder.Services.AddSingleton(new PostmarkMailProviderConfiguration
            {
                DefaultConfiguration = new PostmarkClientConfiguration
                {
                    ApiKey = configurationSection.GetValue<string>("ApiKey") ?? string.Empty,
                    Name = "Default",
                    From = configurationSection.GetValue<string>("From") ?? string.Empty
                },
                MessageStreams = configurationSection.GetValue<List<PostmarkClientConfiguration>>("MessageStreams")
            });
            builder.Services.AddSingleton<PostmarkMailClientFactory>();
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