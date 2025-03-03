using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Extensions;

namespace Passwordless.Service.Models;

public class SigninTokenRequest : RequestBase
{
    private static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromSeconds(120);

    [Required(AllowEmptyStrings = false)]
    public required string UserId { get; init; }

    /// <summary>
    /// Time to live is the number of seconds the token has before it expires.
    /// </summary>
    [JsonPropertyName("timeToLive")]
    [Range(1, 604800)]
    public int? TimeToLiveSeconds { get; init; }

    [JsonIgnore]
    public TimeSpan TimeToLive => TimeToLiveSeconds?.ToTimeSpanFromSeconds() ?? DefaultTimeToLive;

    public string Purpose { get; set; } = SignInPurpose.SignInName;
};