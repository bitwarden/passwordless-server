using System.Text;

namespace Passwordless.AdminConsole.Components.Pages.App.Credentials;

public partial class List : BaseApplicationPage
{
    private readonly IReadOnlyCollection<string> _columnHeaders = new List<string>
    {
        "User Id",
        "Aliases",
        "# Credentials",
        "Last Seen"
    };

    private const string EmptyMessage = "No users found.";

    protected override async Task OnInitializedAsync()
    {
        var users = await PasswordlessClient.ListUsersAsync();
        Users = users.Select(UserViewModel.FromDto).ToList();
        MaxUsers = CurrentContext.Features.MaxUsers;
    }

    public long? MaxUsers { get; private set; }

    public IReadOnlyCollection<UserViewModel>? Users { get; private set; }

    public record UserViewModel(string UserId, string Aliases, int CredentialCount, DateTime? LastSeen)
    {
        public static UserViewModel FromDto(PasswordlessUserSummary dto)
        {
            var aliasesBuilder = new StringBuilder();
            aliasesBuilder.Append($"(${dto.Aliases.Count}) ");
            if (dto.AliasCount > 0)
            {
                aliasesBuilder.Append(' ');
                aliasesBuilder.Append(string.Join(", ", dto.Aliases.Where(a => !string.IsNullOrEmpty(a))));
            }
            return new UserViewModel(dto.UserId, aliasesBuilder.ToString(), dto.CredentialsCount, dto.LastUsedAt);
        }
    }
}