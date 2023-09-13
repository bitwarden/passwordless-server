namespace Passwordless.Common.AuditLog.Enums;

public enum Severity
{
    Unknown = 0,
    Alert = 1, // token modification / things that would be security breach attempt
    Warning = 2, // failed login / expired token / old magic link
    Informational = 3, // most things here
    Error = 5 // internal , misconfiguration , etc.
}