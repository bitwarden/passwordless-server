using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Pages;

public class Initialize(ConsoleDbContext dbContext, IPasswordlessManagementClient managementClient) : PageModel
{
    public string ManagementKey { get; private set; }
    public string AdminEmail { get; private set; }
    public string ApiUrl { get; private set; }

    public CreateAppResultDto CreateAppResultDto { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await dbContext.Database.CanConnectAsync())
        {
            return Page();
        }

        return RedirectToPage("/");
    }

    public void OnGetAsync(CreateAppResultDto keys, string managementKey, string apiUrl)
    {
        CreateAppResultDto = keys;
        ManagementKey = managementKey;
        ApiUrl = apiUrl;
    }

    public async Task OnPostAsync()
    {
        if (string.IsNullOrEmpty(ManagementKey)) return;

        // create http request to create app endpoint using the management key
        var request = new CreateAppDto { AdminEmail = this.AdminEmail };

        using var http = new HttpClient
        {
            BaseAddress = new Uri(ApiUrl),
            DefaultRequestHeaders = { { "ManagementKey", ManagementKey } }
        };

        using var response = await http.PostAsJsonAsync("/admin/apps/AdminConsole/create", request);

        var keys = await response.Content.ReadFromJsonAsync<CreateAppResultDto>();

        OnGetAsync(keys!, ManagementKey, ApiUrl);
    }
}