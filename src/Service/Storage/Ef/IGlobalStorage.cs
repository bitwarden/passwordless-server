namespace Passwordless.Service.Storage.Ef;
using Passwordless.Service.Models;


public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
    Task<ApplicationSummary> GetApplicationSummary(string applicationId);
}