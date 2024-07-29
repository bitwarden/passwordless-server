using System.Runtime.Serialization;

namespace Passwordless.AdminConsole.Models;

public enum UseCaseType
{
    [EnumMember(Value = "customers")]
    Customers,

    [EnumMember(Value = "employees")]
    Employees,

    [EnumMember(Value = "both")]
    Both
}