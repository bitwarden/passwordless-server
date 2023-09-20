using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct ApplicationPublicKey
{
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);
    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public ApplicationPublicKey(string key)
    {
        if (!key.Contains("public")) throw new ArgumentException("Api public keys contain 'public'.");

        Value = key;
    }
}