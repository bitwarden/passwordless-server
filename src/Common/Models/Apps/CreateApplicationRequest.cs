using Microsoft.AspNetCore.Mvc;

namespace Passwordless.Common.Models.Apps;

public class CreateApplicationRequest
{
    [FromBody]
    public required CreateAppDto Payload { get; set; }
}