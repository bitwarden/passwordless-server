namespace Passwordless.Common.Configuration;

public static class WebApplicationBuilderExtensions
{
    public static void AddSelfHostingConfiguration(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("appsettings.SelfHosting.json", true, true);
        }
        else
        {
            builder.Configuration.AddJsonFile("/etc/bitwarden_passwordless", "config.json", true, true);
        }
    }
}