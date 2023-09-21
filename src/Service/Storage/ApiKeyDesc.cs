using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Storage;

public class ApiKeyDesc : PerTenant
{
    public string Id { get; set; }

    [Obsolete]
    public string? AccountName { get; set; }
    public string ApiKey { get; set; }
    public string[] Scopes { get; set; }
    public bool IsLocked { get; set; }

    public DateTime? LastLockedAt { get; set; }
    public DateTime? LastUnlockedAt { get; set; }

    public void CheckLocked()
    {
        if (IsLocked) throw new ApiException("ApiKey has been disabled due to account deletion in process. Please see email to reverse.", 403);
    }
}