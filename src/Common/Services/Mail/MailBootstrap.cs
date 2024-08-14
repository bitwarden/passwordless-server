using System.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.SendGrid;
using Passwordless.Common.Services.Mail.Smtp;

namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMailProviderFactory, MailProviderFactory>();
        builder.Services.AddScoped<IMailProvider, OrderedMailProvider>();
        builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("Mail"))
            .PostConfigure<MailConfiguration>(o =>
            {
                var section = builder.Configuration.GetSection("Mail:Providers");

                // Add a default file provider if no providers are configured.
                if (!section.GetChildren().Any())
                {
                    var fileProviderOptions = new FileMailProviderOptions();
                    o.Providers.Add(fileProviderOptions);
                    return;
                }

                // Iterate over all configured providers and add them to the list with binding.
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

                    o.Providers.Add(mailProviderOptions);
                }
            });
    }
}