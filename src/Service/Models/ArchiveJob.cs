using Passwordless.Common.Models.Backup;

namespace Passwordless.Service.Models;

public class ArchiveJob : PerTenant
{
    public Guid Id { get; set; }

    public AccountMetaInformation Application { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public List<Archive> Archives { get; set; } = new();
}