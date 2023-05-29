using System.Text.Json;

namespace Passwordless.Net.Helpers;

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}