using Passwordless.Common.Extensions;

namespace Passwordless.Common.Models;

public struct ApplicationSecretKey
{
    public string Value { get; }
    public string AbbreviatedValue => Value.GetLast(4);

    public string MaskedValue => string.Join("***", AbbreviatedValue);

    public ApplicationSecretKey(string key)
    {
        if (!key.Contains("secret")) throw new ArgumentException("Api secret keys contain 'secret'.");

        Value = key;
    }
}