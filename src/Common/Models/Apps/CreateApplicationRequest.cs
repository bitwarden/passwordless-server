using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Passwordless.Common.Models.Apps;

public class CreateApplicationRequest
{
    [FromRoute, MinLength(3), RegularExpression("^[a-z]{1}[a-z0-9]{2,61}$")]
    public required string AppId { get; set; }

    [FromBody]
    public required CreateAppDto Payload { get; set; }
}