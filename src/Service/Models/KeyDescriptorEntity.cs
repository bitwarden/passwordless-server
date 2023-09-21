using Microsoft.Azure.Cosmos.Table;

namespace Passwordless.Service.Models;

public class KeyDescriptorEntity : TableEntity
{
    public string Descriptor { get; set; }
}