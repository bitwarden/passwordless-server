using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Licensing.Cryptography;

public class FileCryptographyProvider : ICryptographyProvider
{
    private readonly FileCryptographyConfiguration _options;

    public FileCryptographyProvider(IOptions<FileCryptographyConfiguration> options)
    {
        _options = options.Value;
    }

    public RSA PrivateKey
    {
        get
        {
            string path = _options.PrivateKey;
            string content = File.ReadAllText(path);

            RSA key = RSA.Create();
            key.ImportFromPem(content);

            return key;
        }
    }
}