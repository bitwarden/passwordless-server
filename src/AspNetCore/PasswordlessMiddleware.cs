using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Passwordless.Net;

namespace Passwordless.AspNetCore;

public class PasswordlessEndpointOptions
{
    public string CreateToken { get; set; } = "/passwordless/create-token";
    public string ConvertUser { get; set; } = "/passwordless/convert-user";
    public string VerifySignIn { get; set; } = "/passwordless/verify-signin";
}

public class PasswordlessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PasswordlessEndpointOptions _passwordlessEndpointOptions;
    private readonly IProblemDetailsService _problemDetailsService;

    public PasswordlessMiddleware(
        RequestDelegate next,
        PasswordlessEndpointOptions passwordlessEndpointOptions,
        IProblemDetailsService problemDetailsService
    )
    {
        _next = next;
        _passwordlessEndpointOptions = passwordlessEndpointOptions;
        _problemDetailsService = problemDetailsService;
    }

    public async Task InvokeAsync(HttpContext context, IPasswordlessIdentityService passwordlessIdentityService)
    {
        HttpRequest request = context.Request;

        if (request.Path == _passwordlessEndpointOptions.CreateToken)
        {
            await CreateToken(context, passwordlessIdentityService);
            return;
        }

        if (request.Path == _passwordlessEndpointOptions.VerifySignIn)
        {
            if (await SignInUser(context, passwordlessIdentityService, request))
            {
                return;
            }

            return;
        }

        await _next(context);
    }

    private async Task<bool> SignInUser(HttpContext context,
        IPasswordlessIdentityService passwordlessIdentityService,
        HttpRequest request)
    {
        try
        {
            // Extract the method from the request
            string? token = await ExtractTokenFromRequest(context, request);

            // If token is not present
            if (string.IsNullOrEmpty(token))
            {
                await _problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        // TODO: Add relevant docs
                        Detail =
                            "You must include the verify_token in the request (query paramter, body post, etc), see [docs]"
                    }
                });
                return false;
            }

            // Sign in the user
            VerifiedUser? user = await passwordlessIdentityService.SignInUserAsync(token);
            await context.Response.WriteAsJsonAsync(user, context.RequestAborted);
        }
        catch (Exception ex)
        {
            await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails { Detail = ex.Message },
            });
        }

        return false;
    }

    private static async Task<string?> ExtractTokenFromRequest(HttpContext context, HttpRequest request)
    {
        string? token = null;
        if (request.Query.TryGetValue("token", out StringValues tokenValues))
        {
            token = tokenValues.First();
        }
        else if (request.HasFormContentType)
        {
            IFormCollection form = await request.ReadFormAsync();
            if (form.TryGetValue("token", out tokenValues))
            {
                token = tokenValues.First();
            }
        }
        else if (request.HasJsonContentType())
        {
            JsonDocument? json = await request.ReadFromJsonAsync<JsonDocument>(context.RequestAborted);

            if (json != null)
            {
                if (json.RootElement.TryGetProperty("token", out JsonElement tokenProperty))
                {
                    if (tokenProperty.ValueKind == JsonValueKind.String)
                    {
                        token = tokenProperty.GetString() ?? "";
                    }
                }
            }
        }

        return token;
    }

    private async Task CreateToken(HttpContext context, IPasswordlessIdentityService passwordlessIdentityService)
    {
        string token = await passwordlessIdentityService.ConvertUserAsync();
        await context.Response.WriteAsJsonAsync(token);
        // TODO: Add extensibility
    }
}