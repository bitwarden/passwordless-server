using Microsoft.AspNetCore.Http;
using Passwordless.Net;

namespace Passwordless.AspNetCore;

public class ConvertUserContext
{
    public required HttpContext HttpContext { get; init; }
    public required RegisterOptions RegisterOptions { get; set; }
}