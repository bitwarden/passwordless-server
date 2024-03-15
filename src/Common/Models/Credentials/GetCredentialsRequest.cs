using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Credentials;

public record GetCredentialsRequest([MinLength(1), Required] string UserId);