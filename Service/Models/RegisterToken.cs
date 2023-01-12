using MessagePack;
using Service.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Models
{
    public class RegisterTokenDTO : RegisterToken
    {
        [Key(10)]
        public HashSet<string> Aliases { get; set; }

    }
    [MessagePackObject]
    public class RegisterToken : Token
    {

        [Key(9)]
        public string UserId { get; set; }
        [Key(1)]
        public string DisplayName { get; set; }
        [Key(2)]
        public string Username { get; set; }
        [Key(3)]
        public string AttType { get; set; } = "None";
        [Key(4)]
        public string AuthType { get; set; } = "Platform";
        [Key(5)]
        public bool RequireResidentKey { get; set; } = false;
        [Key(6)]
        public string UserVerification { get; set; } = "Preferred";
    }

    [MessagePackObject]
    public class SignInToken : Token
    {
        [Key(1)]
        public string UserId { get; set; }
        [Key(2)]
        public DateTime Timestamp { get; set; }
        [Key(3)]
        public string RPID { get; set; }
        [Key(4)]
        public string Origin { get; set; }
        [Key(5)]
        public bool Success { get; set; }
        [Key(6)]
        public string Device { get; set; }
        [Key(7)]
        public string Country { get; set; }
        [Key(8)]
        public string Nickname { get; set; }
        [Key(10)]
        public byte[] CredentialId { get; set; }
    }

    [MessagePackObject]
    public class Token
    {
        [Key(0)]
        public DateTime ExpiresAt { get; set; }
        public void Validate()
        {
            var now = DateTime.UtcNow;
            if (ExpiresAt < now)
            {
                var drift = now - ExpiresAt;
                throw new ApiException($"The token expired {drift} ago.");
            }
        }
    }
}
