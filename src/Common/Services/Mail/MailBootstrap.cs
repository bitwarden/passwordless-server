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
        builder.Services.AddScoped<IMailProvider, OrderedMailProvider>();
        builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("Mail"))
            .PostConfigure<MailConfiguration>(o =>
            {
                var section = builder.Configuration.GetSection("Mail:Providers");

                builder.Services.RemoveAll<IMailProvider>();
                builder.Services.AddScoped<IMailProvider, OrderedMailProvider>();

                if (!section.GetChildren().Any())
                {
                    var fileProviderOptions = new FileProviderOptions();
                    o.Providers.Add(new KeyValuePair<string, IProviderOptions>(FileProviderOptions.Provider, fileProviderOptions));
                    builder.Services.AddKeyedSingleton<IMailProvider, FileMailProvider>(0);
                    return;
                }

                var i = 0;
                foreach (var child in section.GetChildren())
                {
                    var type = child.GetValue<string>("Name")!;
                    IProviderOptions providerOptions = type.ToLowerInvariant() switch
                    {
                        AwsProviderOptions.Provider => new AwsProviderOptions(),
                        SendGridProviderOptions.Provider => new SendGridProviderOptions(),
                        SmtpProviderOptions.Provider => new SmtpProviderOptions(),
                        FileProviderOptions.Provider => new FileProviderOptions(),
                        _ => throw new ConfigurationErrorsException($"Unknown mail provider type '{type}'")
                    };
                    child.Bind(providerOptions);
                    o.Providers.Add(new KeyValuePair<string, IProviderOptions>(type, providerOptions));
                    builder.Services.AddKeyedSingleton<IMailProvider, AwsMailProvider>(i);
                    i++;
                }
            });
    }
}