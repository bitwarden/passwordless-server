using Microsoft.Extensions.FileProviders;

namespace Passwordless.Common.Configuration;

public static class IConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonFile(
        this IConfigurationBuilder builder,
        string directory,
        string fileName,
        bool optional = true,
        bool reloadOnChange = true)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentNullException(nameof(directory));
        }
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }
        if (!optional && !Directory.Exists(directory))
        {
            throw new ArgumentException($"'{fileName}' is required, but its directory doesn't exist.");
        }
        var fileProvider = new PhysicalFileProvider(directory);
        return builder.AddJsonFile(fileProvider, fileName, optional, reloadOnChange);
    }
}