using System.Text.Json.Serialization;

namespace Passwordless.Net;

public sealed class PasswordlessApiException : HttpRequestException
{
    public ProblemDetails Details { get; }

    public PasswordlessApiException(ProblemDetails problemDetails) : base(problemDetails.Title)
    {
        Details = problemDetails;
    }
}

public class ProblemDetails
{
    // TODO: Make immutable
    // TODO: Include errorCode as a property once it's more common
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?> Extensions { get; set; }
}