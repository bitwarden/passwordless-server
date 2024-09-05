using System.Configuration;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.SendGrid;
using Passwordless.Common.Services.Mail.Smtp;
using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMailProviderFactory, MailProviderFactory>();
        builder.Services.AddScoped<IMailProvider, AggregateMailProvider>();
        builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("Mail"))
            .PostConfigure<MailConfiguration>(o =>
            {
                var section = builder.Configuration.GetSection("Mail:Providers");

                if (!section.GetChildren().Any())
                {
                    o.Providers = new List<BaseMailProviderOptions>
                    {
                        new FileMailProviderOptions()
                    };
                    return;
                }

                // Iterate over all configured providers and add them to the list with binding.

                var providers = new List<BaseMailProviderOptions>();
                foreach (var child in section.GetChildren())
                {
                    var type = child.GetValue<string>("Name");

                    if (type == null)
                    {
                        throw new ConfigurationErrorsException("Provider type is missing");
                    }

                    BaseMailProviderOptions mailProviderOptions = type.ToLowerInvariant() switch
                    {
                        AwsMailProviderOptions.Provider => new AwsMailProviderOptions(),
                        SendGridMailProviderOptions.Provider => new SendGridMailProviderOptions(),
                        SmtpMailProviderOptions.Provider => new SmtpMailProviderOptions(),
                        FileMailProviderOptions.Provider => new FileMailProviderOptions(),
                        _ => throw new ConfigurationErrorsException($"Unknown mail provider type '{type}'")
                    };

                    // This will allow our configuration to update without having to restart the application.
                    child.Bind(mailProviderOptions);

                    if (!mailProviderOptions.Channels.ContainsKey(Channel.Default))
                    {
                        throw new ConfigurationErrorsException("Default channel configuration is missing");
                    }

                    providers.Add(mailProviderOptions);
                }
                o.Providers = providers;
            });
    }
}