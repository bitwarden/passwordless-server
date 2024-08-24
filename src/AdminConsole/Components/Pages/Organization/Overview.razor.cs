using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Overview : ComponentBase
{
    private string? _title;

    private bool? _canCreateApplication;

    private IReadOnlyCollection<ApplicationViewModel>? _applications;

    protected override async Task OnInitializedAsync()
    {
        _canCreateApplication = await DataService.AllowedToCreateApplicationAsync();
        var organization = await DataService.GetOrganizationWithDataAsync();
        if (organization == null)
        {
            throw new InvalidOperationException("Organization not found");
        }

        _title = $"{organization.Name} Applications";
        _applications = organization.Applications
            .Select(x => new ApplicationViewModel(
                x.Id,
                x.Name,
                x.Description,
                x.CreatedAt,
                x.DeleteAt,
                x.CurrentUserCount))
            .ToList();
    }

    private record ApplicationViewModel(
        string Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime? DeleteAt,
        int Users)
    {
        public string ListItemIdentifier => $"{Id}-list-item";
        public string NameIdentifier => $"{Id}-name";
        public string DescriptionIdentifier => $"{Id}-description";
        public string CreatedAtIdentifier => $"{Id}-created-at";
        public string DeleteAtIdentifier => $"{Id}-delete-at";
        public string Url => DeleteAt.HasValue ? $"/app/{Id}/settings" : $"/app/{Id}/credentials/list";
    }
}