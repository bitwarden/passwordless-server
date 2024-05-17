using System.Security.Claims;

namespace Passwordless.AdminConsole.Services;

public interface IPostSignInHandlerService
{
    Task HandleAsync();
}