using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
    Task<ApplicationSummary> GetApplicationSummary(string applicationId);
}