using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Models;

public class Archive : PerTenant
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Type? Entity { get; set; }

    [MaxLength(100 * 1024 * 1024, ErrorMessage = "Data cannot be larger than 100MB.")]
    public byte[] Data { get; set; }

    public AccountMetaInformation Application { get; set; } = null!;
}