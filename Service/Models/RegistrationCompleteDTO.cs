using System.Collections.Generic;
using Fido2NetLib;

namespace Service.Models
{
    public class RegistrationCompleteDTO : RequestBase
    {
        public string SessionId { get; set; }

        public AuthenticatorAttestationRawResponse Response { get; set; }
        public string Nickname { get; set; }
    }
}
