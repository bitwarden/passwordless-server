using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public interface IStorage
{
    Task<ICollection<ApplicationPendingDeletion>> GetApplicationsPendingDeletionAsync();
}