namespace Passwordless.Service.MagicLinks;

public class MagicLinksOptions
{
    public TimeSpan NewAccountTimeout { get; set; } = TimeSpan.FromHours(24);
}