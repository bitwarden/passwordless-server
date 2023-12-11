﻿using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;
using Passwordless.Common.Constants;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class CreateSecretKeyModel : BaseExtendedPageModel
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ICurrentContext _currentContext;
    private readonly ILogger<SettingsModel> _logger;

    public CreateSecretKeyModel(
        IPasswordlessManagementClient managementClient,
        ICurrentContext currentContext,
        ILogger<SettingsModel> logger
        )
    {
        _managementClient = managementClient;
        _currentContext = currentContext;
        _logger = logger;
    }

    public CreateApiKeyModel Model { get; } = new(ApiKeyScopes.SecretScopes);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var selectedScopesValue = Request.Form[Model.SelectedScopesField].ToString();
        if (string.IsNullOrEmpty(selectedScopesValue))
        {
            ModelState.AddModelError("error", "Please select at least one scope.");
            return Page();
        }

        var selectedScopes = selectedScopesValue.Split(',').ToHashSet();

        try
        {
            var request = new CreateApiKeyRequest(ApiKeyTypes.Public, selectedScopes);
            await _managementClient.CreateApiKeyAsync(_currentContext.AppId!, request);

            return RedirectToApplicationPage("/App/Settings/Settings", new ApplicationPageRoutingContext(_currentContext.AppId!));
        }
        catch (Exception)
        {
            _logger.LogError("Failed to create secret key for application: {appId}", _currentContext.AppId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }
}