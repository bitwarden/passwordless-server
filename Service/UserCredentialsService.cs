using Microsoft.Extensions.Configuration;
using Service.Models;
using Service.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Helpers
{
    public class UserCredentialsService
    {
        private readonly IStorage _storage;

        public UserCredentialsService(string tenant, IConfiguration config, IStorage storage)
        {
            _storage = storage;
        }

        public async Task<StoredCredential[]> GetAllCredentials(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("userId must not be null or empty", 400);
            }

            var credIds = await _storage.GetCredentialsByUserIdAsync(userId);

            var tasks = credIds.Select(c => _storage.GetCredential(c.Id));
            var creds = await Task.WhenAll(tasks);

            return creds;
        }

        public async Task DeleteCredential(byte[] credentialId)
        {
            if (credentialId == null || credentialId.Length == 0)
            {
                throw new ApiException("credentialId must not be null or empty", 400);
            }
            await _storage.DeleteCredential(credentialId);
        }
    }
}
