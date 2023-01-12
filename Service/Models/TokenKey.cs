using System;

namespace Service.Models
{
    public class TokenKey
    {
        public string KeyMaterial { get; set; }
        public int KeyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}