namespace Passwordless.Service.Storage.Ef;
public interface IGlobalStorage
{
    Task<ICollection<string>> GetApplicationsPendingDeletionAsync();
    Task<int> UpdatePeriodicCredentialReportsAsync();
    Task<int> UpdatePeriodicActiveUserReportsAsync();
    Task DeleteOldDispatchedEmailsAsync(TimeSpan age);
}