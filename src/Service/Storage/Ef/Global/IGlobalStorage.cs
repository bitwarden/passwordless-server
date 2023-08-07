namespace Passwordless.Service.Storage.Ef.Global;

public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
}