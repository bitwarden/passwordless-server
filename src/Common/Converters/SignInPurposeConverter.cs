using System.Text.Json;
using System.Text.Json.Serialization;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Common.Converters;

public class SignInPurposeConverter : JsonConverter<SignInPurpose>
{
    public override SignInPurpose? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var signInPurpose = reader.GetString();

        return string.IsNullOrWhiteSpace(signInPurpose)
            ? null
            : new SignInPurpose(signInPurpose);
    }

    public override void Write(Utf8JsonWriter writer, SignInPurpose value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}