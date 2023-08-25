namespace Passwordless.Common.AuditLog.Enums;

public enum Severity
{
    Alert, // token modification / things that would be security breach attempt
    Warning, // failed login / expired token / old magic link
    Informational, // most things here
    Error // internal , misconfiguration , etc.
}