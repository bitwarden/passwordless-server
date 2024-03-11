namespace Passwordless.Common.Models.Backup;

public enum JobStatus : short
{
    Pending = 0,
    Failed = -1,
    Running = 1,
    Completed = 2
}