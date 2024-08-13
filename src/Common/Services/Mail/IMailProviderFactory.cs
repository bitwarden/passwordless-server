namespace Passwordless.Common.Services.Mail;

public interface IMailProviderFactory
{
    IMailProvider Create(string name, BaseProviderOptions options);
}