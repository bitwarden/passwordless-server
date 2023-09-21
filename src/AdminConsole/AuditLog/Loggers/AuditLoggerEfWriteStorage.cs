using AdminConsole.Db;
namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerEfWriteStorage : AbstractAuditLoggerEfWriteStorage
{
    public AuditLoggerEfWriteStorage(ConsoleDbContext context) : base(context)
    {
    }
}