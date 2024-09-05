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

                    switch (type.ToLowerInvariant())
                    {
                        case AwsMailProviderOptions.Provider:
                            var awsProviderOptions = new AwsMailProviderOptions();
                            child.Bind(awsProviderOptions);
                            if (!awsProviderOptions.Channels.Any())
                            {
                                if (o.From == null)
                                {
                                    throw new ConfigurationErrorsException("From is missing");
                                }

                                awsProviderOptions.Channels.Add(Channel.Default,
                                    new AwsChannelOptions { From = o.From, FromName = o.FromName });
                            }

                            providers.Add(awsProviderOptions);
                            break;
                        case FileMailProviderOptions.Provider:
                            var fileProviderOptions = new FileMailProviderOptions();
                            child.Bind(fileProviderOptions);
                            if (!fileProviderOptions.Channels.Any())
                            {
                                if (o.From == null)
                                {
                                    throw new ConfigurationErrorsException("From is missing");
                                }

                                fileProviderOptions.Channels.Add(Channel.Default,
                                    new ChannelOptions { From = o.From, FromName = o.FromName });
                            }

                            providers.Add(fileProviderOptions);
                            break;
                        case SendGridMailProviderOptions.Provider:
                            var sendGridProviderOptions = new SendGridMailProviderOptions();
                            child.Bind(sendGridProviderOptions);
                            if (!sendGridProviderOptions.Channels.Any())
                            {
                                if (o.From == null)
                                {
                                    throw new ConfigurationErrorsException("From is missing");
                                }

                                sendGridProviderOptions.Channels.Add(Channel.Default,
                                    new ChannelOptions { From = o.From, FromName = o.FromName });
                            }

                            providers.Add(sendGridProviderOptions);
                            break;
                        case SmtpMailProviderOptions.Provider:
                            var smtpProviderOptions = new SmtpMailProviderOptions();
                            child.Bind(smtpProviderOptions);
                            if (!smtpProviderOptions.Channels.Any())
                            {
                                if (o.From == null)
                                {
                                    throw new ConfigurationErrorsException("From is missing");
                                }

                                smtpProviderOptions.Channels.Add(Channel.Default,
                                    new ChannelOptions { From = o.From, FromName = o.FromName });
                            }

                            providers.Add(smtpProviderOptions);
                            break;
                    }
                }

                o.Providers = providers;
            });
    }
}