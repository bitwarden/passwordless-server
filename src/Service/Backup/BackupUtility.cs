using Passwordless.Common.Backup;

namespace Passwordless.Service.Backup;

public class BackupUtility : IBackupUtility
{
    /**
     * 1. AccountMetaInformation
     * 2. ApiKeyDesc
     * 3. AppFeature
     * 4. Authenticator
     * 5. AliasPointer
     * 6. EFStoredCredential
     * L. ApplicationEvent
     * L. PeriodicCredentialReport
     * L. PeriodicActiveUserReport
     * Ignore: DispatchedEmail
     * Ignore: TokenKey
     */
}