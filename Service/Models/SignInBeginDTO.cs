using System.Collections.Generic;

namespace Service.Models
{
    public class SignInBeginDTO : RequestBase
    {
        public string Alias { get; set; }
        public string UserId { get; set; }
        public string UserVerification { get; set; }
    }
}
