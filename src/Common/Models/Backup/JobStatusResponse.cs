namespace Passwordless.Common.Models.Backup;

public enum JobStatusResponse : short
{
    Failed = -1,
    Pending = 0,
    Running = 1,
    Completed = 2
}