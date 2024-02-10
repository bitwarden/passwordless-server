using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;
public interface IGlobalStorage
{
    Task<ApiKeyDesc> GetApiKeyAsync(string apiKey);
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
    Task<int> UpdatePeriodicCredentialReportsAsync();
}