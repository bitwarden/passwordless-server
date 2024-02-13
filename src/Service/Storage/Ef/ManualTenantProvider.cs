namespace Passwordless.Service.Storage.Ef;

public record ManualTenantProvider(string Tenant) : ITenantProvider;