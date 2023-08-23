using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;
using ProblemDetails = Passwordless.Net.ProblemDetails;

namespace AdminConsole.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);


    public ErrorModel()
    {
    }

    public string? RequestId { get; set; }

    public string Message { get; set; }

    public ProblemDetails? ProblemDetails { get; set; }

    public void OnGet(string message)
    {
        Message = message;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = (PasswordlessApiException)exceptionHandlerPathFeature?.Error;
        ProblemDetails = exception?.Details;
    }
}