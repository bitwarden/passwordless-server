using System.ComponentModel.DataAnnotations;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public class PasswordlessManagementOptions
{
    /// <summary>
    /// When using self-hosted Docker containers, the AdminConsole will communicate directly with the Passwordless API inside the container.
    /// </summary>
    public string InternalApiUrl { get; set; }

    [Required]
    public string ManagementKey { get; set; }

    /// <summary>
    /// The client is not aware of any networking inside our container when using self-hosting. So we need to provide the external URL.
    /// </summary>
    [Required]
    public string ApiUrl { get; set; }
}