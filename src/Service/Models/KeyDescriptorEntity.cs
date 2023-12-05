using Microsoft.Azure.Cosmos.Table;

namespace Passwordless.Service.Models;

public class KeyDescriptorEntity : TableEntity
{
    public required string Descriptor { get; set; }
}