namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Responsible for creating instances of <see cref="IMailProvider"/>.
/// </summary>
public interface IMailProviderFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IMailProvider"/>.
    /// </summary>
    /// <param name="name">The name of the provider (case insensitive).</param>
    /// <param name="options">The options to use when creating the provider.</param>
    /// <returns></returns>
    IMailProvider Create(string name, BaseMailProviderOptions options);
}