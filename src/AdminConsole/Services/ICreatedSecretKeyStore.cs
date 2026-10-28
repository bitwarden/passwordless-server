namespace Passwordless.AdminConsole.Services;

/// <summary>
/// Hands a newly created secret key from the page that created it to the page that displays it, without the key
/// ever appearing in a URL.
/// </summary>
public interface ICreatedSecretKeyStore
{
    /// <summary>
    /// Stores the secret key that was just created for the application so it can be shown once on the next request.
    /// </summary>
    void Stash(string appId, string apiKey);

    /// <summary>
    /// Returns the secret key stashed for the application and removes it, or <c>null</c> when there is none.
    /// </summary>
    string? Take(string appId);
}