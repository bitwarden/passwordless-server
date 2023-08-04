namespace Passwordless.Service.Storage.Ef;

public interface IStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
}