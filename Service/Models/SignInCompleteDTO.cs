using System.Collections.Generic;
using Fido2NetLib;

namespace Service.Models
{
    public class SignInCompleteDTO : RequestBase
    {
        public AuthenticatorAssertionRawResponse Response { get; set; }
        public string SessionId { get; set; }
    }
}
