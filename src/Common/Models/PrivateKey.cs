using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct PrivateKey
{
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);

    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public PrivateKey(string key)
    {
        if (!key.Contains("secret")) throw new ArgumentException("Api secret keys contain 'secret'.");

        Value = key;
    }
}