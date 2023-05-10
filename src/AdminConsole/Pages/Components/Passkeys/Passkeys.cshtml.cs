using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace AdminConsole.Pages.Components.Passkeys;

public class Passkeys : ViewComponent
{
    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        return View();
    }
}