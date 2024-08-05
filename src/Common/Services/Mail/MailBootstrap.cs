namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<MailConfiguration>().BindConfiguration("Mail");

        if (builder.Configuration.GetSection("Mail:Postmark").Exists())
        {
            var configurationSection = builder.Configuration.GetSection("Mail:PostMark");

            var clients = new List<PostmarkClientConfiguration>();
            configurationSection.GetSection("MessageStreams").Bind(clients);

            builder.Services.AddSingleton(new PostmarkMailProviderConfiguration
            {
                DefaultConfiguration = new PostmarkClientConfiguration
                {
                    ApiKey = configurationSection.GetValue<string>("ApiKey") ?? string.Empty,
                    Name = "Default",
                    From = configurationSection.GetValue<string>("From") ?? string.Empty
                },
                MessageStreams = clients
            });

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