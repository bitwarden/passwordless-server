namespace Passwordless.AdminConsole.Models.DTOs;

public record ApplicationsPendingDeletionResponse(IEnumerable<ApplicationPendingDeletion> ApplicationPendingDeletions);
public record ApplicationPendingDeletion(string Tenant, DateTime DeleteAt);