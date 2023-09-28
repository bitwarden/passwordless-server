using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using static Passwordless.Service.AuditLog.AuditEventFunctions;

namespace Passwordless.Service;

public class UserCredentialsService
{
    private readonly ITenantStorage _storage;
    private readonly IAuditLogger _auditLogger;

    public UserCredentialsService(ITenantStorage storage,
        IAuditLogger auditLogger)
    {
        _storage = storage;
        _auditLogger = auditLogger;
    }

    public async Task<StoredCredential[]> GetAllCredentials(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ApiException("missing_userid", "userId must not be null or empty", 400);
        }

        var creds = await _storage.GetCredentialsByUserIdAsync(userId);

        return creds.ToArray();
    }

    public async Task DeleteCredential(byte[] credentialId)
    {
        if (credentialId == null || credentialId.Length == 0)
        {
            throw new ApiException("credentialId must not be null or empty", 400);
        }

        var credential = await _storage.GetCredential(credentialId);

        await _storage.DeleteCredential(credentialId);

        _auditLogger.LogEvent(DeleteCredentialEvent(credential.UserId));
    }

    public Task<List<UserSummary>> GetAllUsers(string paginationLastId)
    {
        return _storage.GetUsers(paginationLastId);
    }

    public Task<int> GetUsersCount()
    {
        return _storage.GetUsersCount();
    }

    public Task DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ApiException("userId must not be null or empty", 400);
        }

        return _storage.DeleteUser(userId);
    }
}