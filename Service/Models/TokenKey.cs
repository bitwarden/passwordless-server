using System;

namespace Service.Models
{
    public class TokenKey : PerTenant
    {
        public string KeyMaterial { get; set; }
        public int KeyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}