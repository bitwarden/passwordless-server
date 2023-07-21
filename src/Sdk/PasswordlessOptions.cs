namespace Passwordless.Net;

/// <summary>
/// Represents all the options you can use to configure a backend Passwordless system.
/// </summary>
public class PasswordlessOptions
{
    /// <summary>
    /// Passwordless Cloud Url
    /// </summary>
    public const string CloudApiUrl = "https://v4.passwordless.dev";

    /// <summary>
    /// Gets or sets the url to use for Passwordless operations.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="CloudApiUrl" />.
    /// </remarks>
    public string ApiUrl { get; set; } = CloudApiUrl;

    /// <summary>
    /// Gets or sets the secret API key used to authenticate with the Passwordless API.
    /// </summary>
    public string ApiSecret { get; set; } = default!;

    /// <summary>
    /// Gets or sets the public API key used to interact with the Passwordless API.
    /// </summary>
    /// <remarks>
    /// Optional: Only used for frontend operations by the JS Client. E.g: Useful if you're using MVC/Razor pages
    /// </remarks>
    public string? ApiKey { get; set; }
}