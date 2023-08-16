using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;
public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(SetFeaturesBulkDto payload);
}