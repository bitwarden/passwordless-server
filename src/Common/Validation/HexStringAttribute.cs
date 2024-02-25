using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class HexStringAttribute() : RegularExpressionAttribute("^(0x)?([0-9A-Fa-f]{2})+$");