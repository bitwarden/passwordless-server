namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Mail:Postmark").Exists())
        {
            var configurationSection = builder.Configuration.GetSection("Mail:PostMark");

            var postmarkConfiguration = new PostmarkMailProviderConfiguration
            {
                DefaultConfiguration = new PostmarkClientConfiguration
                {
                    ApiKey = configurationSection.GetValue<string>("ApiKey"),
                    Name = "Default",
                    From = configurationSection.GetValue<string>("From")
                },
                MessageStreams = configurationSection.GetValue<List<PostmarkClientConfiguration>>("MessageStreams")
            };

            builder.Services.AddSingleton(postmarkConfiguration);
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