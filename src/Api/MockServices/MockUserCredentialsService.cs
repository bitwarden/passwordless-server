using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Passwordless.Service;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Api.MockServices;

public class MockUserCredentialsService : IUserCredentialsService
{
    public Task DeleteCredential(byte[] credentialId)
    {
        if (credentialId == null || credentialId.Length == 0)
        {
            return Task.FromException(new ApiException("credentialId must not be null or empty", 400));
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public Task DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromException(new ApiException("userId must not be null or empty", 400));
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public Task<StoredCredential[]> GetAllCredentials(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromException<StoredCredential[]>(new ApiException("missing_userid", "userId must not be null or empty", 400));
        }

        if (userId == "has_credentials")
        {
            return Task.FromResult(new []
            {
                new StoredCredential
                {
                    AttestationFmt = "attestation_fmt",
                    Country = "US",
                    CreatedAt = DateTime.UtcNow,
                    UserHandle = Encoding.UTF8.GetBytes(userId),
                    Descriptor = new Fido2NetLib.Objects.PublicKeyCredentialDescriptor
                    {
                        Id = new byte[] { 0x2, 0x3 },
                        Type = Fido2NetLib.Objects.PublicKeyCredentialType.PublicKey,
                        Transports = new []
                        {
                            AuthenticatorTransport.Nfc,
                        }
                    },
                    Device = "device",
                    LastUsedAt = DateTime.UtcNow,
                    Nickname = "my_nickname",
                    Origin = "localhost",
                    SignatureCounter = 5,
                    PublicKey = new byte [] { 0x1, 0x2 },
                    RPID = "https://localhost"
                }
            });
        }
        else
        {
            return Task.FromResult(Array.Empty<StoredCredential>());
        }
    }

    public Task<List<UserSummary>> GetAllUsers(string paginationLastId)
    {
        return Task.FromResult(new List<UserSummary>
        {
            new UserSummary
            {
                UserId = "test_user_id_one",
                AliasCount = 1,
                Aliases = new List<string> { "test_alias_1" },
                CredentialsCount = 2,
                LastUsedAt = DateTime.UtcNow,
            },
            new UserSummary
            {
                UserId = "test_user_id_two",
                AliasCount = 1,
                Aliases = new List<string> { "test_alias_2" },
                CredentialsCount = 0,
                LastUsedAt = null,
            },
        });
    }

    public Task<int> GetUsersCount()
    {
        return Task.FromResult(2);
    }
}
