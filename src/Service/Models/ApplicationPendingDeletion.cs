namespace Passwordless.Service.Models;

public record ApplicationPendingDeletion(string Tenant, DateTime DeleteAt);