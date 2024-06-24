using System.Text.Json.Serialization;
using Passwordless.Common.Converters;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class SignInBeginDTO : RequestBase
{
    public string Alias { get; set; }
    public string UserId { get; init; }
    [JsonConverter(typeof(SignInPurposeConverter))]
    public SignInPurpose Purpose { get; set; } = SignInPurpose.SignIn;
}