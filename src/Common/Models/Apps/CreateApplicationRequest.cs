using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Passwordless.Common.Models.Apps;

public class CreateApplicationRequest
{
    [FromRoute, MinLength(3)]
    [RegularExpression("^[a-z]{1}[a-z0-9]{2,61}$", ErrorMessage = "'AppId' must be between 3 and 62 characters, contain only lowercase letters and numbers, and start with a lowercase letter.")]
    public required string AppId { get; set; }

    [FromBody]
    public required CreateAppDto Payload { get; set; }
}