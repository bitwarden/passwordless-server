namespace Passwordless.AdminConsole.Services;

public interface IPostSignInHandlerService
{
    Task HandleAsync(int organizationId);
}