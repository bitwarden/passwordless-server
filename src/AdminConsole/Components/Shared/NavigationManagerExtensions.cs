using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

public static class NavigationManagerExtensions
{
    /// <summary>
    /// Refreshes the current page. Required for .NET 8.0.1 runtime and below.
    /// </summary>
    /// <param name="navigationManager"></param>
    public static void RefreshCompat(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);
    }
}