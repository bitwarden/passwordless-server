using Microsoft.Extensions.Configuration;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef.Tenant;

namespace Passwordless.Service;

public class UserCredentialsService
{
    private readonly ITenantStorage _storage;

    public UserCredentialsService(ITenantStorage storage)
    {
        _storage = storage;
    }

    public UserCredentialsService(string tenant, IConfiguration config, ITenantStorage storage)
    {
        _storage = storage;
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
        await _storage.DeleteCredential(credentialId);
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

public class UserSummary
{
    public string UserId { get; set; }
    public int AliasCount { get; set; }
    public List<string> Aliases { get; set; }
    public int CredentialsCount { get; set; }
    public DateTime LastUsedAt { get; set; }
}