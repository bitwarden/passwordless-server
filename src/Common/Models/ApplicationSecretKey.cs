using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct ApplicationSecretKey
{
    public const string KeyIdentifier = ":secret:";
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);

    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public ApplicationSecretKey(string key)
    {
        if (!key.Contains(KeyIdentifier)) throw new ArgumentException("Api secret keys contain 'secret'.");

        Value = key;
    }
}