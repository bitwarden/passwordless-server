using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct ApplicationPublicKey
{
    public const string KeyIdentifier = ":public:";
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);
    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public ApplicationPublicKey(string key)
    {
        if (!key.Contains(KeyIdentifier)) throw new ArgumentException("Api public keys contain 'public'.");

        Value = key;
    }
}