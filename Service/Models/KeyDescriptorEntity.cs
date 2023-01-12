using Microsoft.Azure.Cosmos.Table;

namespace Service.Models
{
    public class KeyDescriptorEntity : TableEntity
    {
        public string Descriptor { get; set; }
    }
}
