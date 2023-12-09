namespace Passwordless.Service.Models;

public class ApiKeyDto
{
    /// <summary>
    /// Last 4 digits of the API key
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// API key type
    /// </summary>
    public ApiKeyTypes Type { get; set; }

    /// <summary>
    /// Permissions granted to the API key
    /// </summary>
    public HashSet<string> Scopes { get; set; }

    /// <summary>
    /// Whether the API key is locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Date the API Key was last locked, null if never.
    /// </summary>
    public DateTime? LastLockedAt { get; set; }

    /// <summary>
    /// Date the API Key was last unlocked, null if never.
    /// </summary>
    public DateTime? LastUnlockedAt { get; set; }
}