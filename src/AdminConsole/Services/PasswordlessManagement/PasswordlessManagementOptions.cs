using System.ComponentModel.DataAnnotations;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public class PasswordlessManagementOptions
{
    [Required]
    public string ManagementKey { get; set; }

    /// <summary>
    /// The client is not aware of any networking inside our container when using self-hosting. So we need to provide the external URL.
    /// </summary>
    [Required]
    public string ApiUrl { get; set; }
}