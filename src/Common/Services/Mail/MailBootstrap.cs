using System.Configuration;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.SendGrid;
using Passwordless.Common.Services.Mail.Smtp;

namespace Passwordless.Common.Services.Mail;

public static class MailBootstrap
{
    public static void AddMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMailProvider, OrderedMailProvider>();
        builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("Mail"))
            .PostConfigure<MailConfiguration>(o =>
            {
                var section = builder.Configuration.GetSection("Mail:Providers");

                if (!section.GetChildren().Any())
                {
                    var fileProviderOptions = new FileProviderOptions();
                    o.Providers.Add(fileProviderOptions);
                    return;
                }

                foreach (var child in section.GetChildren())
                {
                    var type = child.GetValue<string>("Name");

                    if (type == null)
                    {
                        throw new ConfigurationErrorsException("Provider type is missing");
                    }

                    BaseProviderOptions providerOptions = type.ToLowerInvariant() switch
                    {
                        AwsProviderOptions.Provider => new AwsProviderOptions(),
                        SendGridProviderOptions.Provider => new SendGridProviderOptions(),
                        SmtpProviderOptions.Provider => new SmtpProviderOptions(),
                        FileProviderOptions.Provider => new FileProviderOptions(),
                        _ => throw new ConfigurationErrorsException($"Unknown mail provider type '{type}'")
                    };
                    child.Bind(providerOptions);
                    o.Providers.Add(providerOptions);
                }
            });
    }
}