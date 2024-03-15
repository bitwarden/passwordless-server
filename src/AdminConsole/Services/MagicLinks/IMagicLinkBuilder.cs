namespace Passwordless.AdminConsole.Services.MagicLinks;

public interface IMagicLinkBuilder
{
    Task<string> GetLinkAsync(string email, string? returnUrl = null);
    string GetUrlTemplate(string? returnUrl = null);
}