using System.Collections.Generic;

namespace Service.Models
{
    public class SignInVerifyDTO : RequestBase
    {
        public string Token { get; set; }
    }
}
