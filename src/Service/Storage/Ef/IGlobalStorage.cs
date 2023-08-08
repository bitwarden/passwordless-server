namespace Passwordless.Service.Storage.Ef;
public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
}