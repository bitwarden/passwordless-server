using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef.AuditLog;

public class DbAuditSqliteContext : DbAuditLogContext
{
    public DbAuditSqliteContext(DbContextOptions options) : base(options)
    {
    }
}