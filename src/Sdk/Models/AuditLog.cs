namespace Passwordless.Net;

public class AuditLog
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
}