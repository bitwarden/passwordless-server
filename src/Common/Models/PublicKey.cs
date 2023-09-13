using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct PublicKey
{
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);
    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public PublicKey(string key)
    {
        if (!key.Contains("public")) throw new ArgumentException("Api public keys contain 'public'.");

        Value = key;
    }
}