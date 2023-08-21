using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef.AuditLog;

public class DbAuditMsSqlContext : DbAuditLogContext
{
    public DbAuditMsSqlContext(DbContextOptions options) : base(options)
    {
    }
}