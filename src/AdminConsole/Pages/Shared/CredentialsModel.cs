namespace Passwordless.AdminConsole.Pages.Shared;

public sealed class CredentialsModel
{
    /// <summary>
    /// The list of credentials.
    /// </summary>
    public IReadOnlyCollection<Credential> Items { get; set; } = new List<Credential>();

    /// <summary>
    /// Determines whether the details of the credentials should be hidden.
    /// </summary>
    public bool HideDetails { get; set; }
}