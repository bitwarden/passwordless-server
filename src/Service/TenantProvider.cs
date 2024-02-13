#nullable enable

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Passwordless.Service;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Tenant
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var accountName = httpContext?.User.FindFirstValue("accountName");
            return accountName ?? throw new InvalidOperationException("Tenant not found");
        }
    }
}