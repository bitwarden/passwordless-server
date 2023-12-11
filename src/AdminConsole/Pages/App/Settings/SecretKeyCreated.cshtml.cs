using System.Text;
using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RoutingHelpers;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SecretKeyCreatedModel : BaseExtendedPageModel
{
    private readonly ICurrentContext _currentContext;

    public SecretKeyCreatedModel(ICurrentContext currentContext)
    {
        _currentContext = currentContext;
    }

    public string ApiKey { get; private set; }

    public IActionResult OnGet()
    {
        var encodedApiKey = Request.Query["EncodedApiKey"].ToString();
        if (string.IsNullOrEmpty(encodedApiKey))
        {
            return RedirectToApplicationPage("/App/Settings/Settings", new ApplicationPageRoutingContext(_currentContext.AppId));
        }
        ApiKey = Encoding.UTF8.GetString(Base64Url.Decode(encodedApiKey));
        return Page();
    }
}