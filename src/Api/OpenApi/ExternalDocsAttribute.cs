namespace Passwordless.Api.OpenApi;

/// <summary>
/// Specifies the URL for the external documentation for this endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ExternalDocsAttribute(string url) : Attribute
{
    /// <summary>
    /// External documentation URL.
    /// </summary>
    public string Url { get; } = url;
}