using System.Runtime.Serialization;

namespace Passwordless.AdminConsole.Models;

public enum OrganizationType
{
    [EnumMember(Value = "personal")]
    Personal = 1,

    [EnumMember(Value = "company")]
    Company = 2,
}