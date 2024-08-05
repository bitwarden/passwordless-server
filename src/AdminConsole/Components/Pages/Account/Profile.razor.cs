using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Components.Pages.Account;

public partial class Profile : ComponentBase
{
    public string UserId => HttpContextAccessor.HttpContext!.User.GetId();
}