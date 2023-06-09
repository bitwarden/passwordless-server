using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Passwordless.Net;

namespace Passwordless.AspNetCore;

/// <summary>
/// Represents all the options you can use to configure Passwordless in your ASP.NET Core application.
/// </summary>
public class PasswordlessAspNetCoreOptions : PasswordlessOptions
{
    /// <summary>
    /// Controls the scheme that will be used to handle the sign in, fallsback to using <see cref="AuthenticationOptions.DefaultSignInScheme" />
    /// that can be configured during <c>AddAuthentication</c> and if that is null will fallback to <see cref="AuthenticationOptions.DefaultScheme" />.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null" />
    /// </remarks>
    public string? SignInScheme { get; set; }

    /// <summary>
    /// Controls the claim type that will be used to find the user id from an authenticated users <see cref="ClaimsPrincipal" />
    /// if it is null it will fallback to <see cref="ClaimsIdentityOptions.UserIdClaimType" /> from <see cref="IdentityOptions.ClaimsIdentity" />
    /// and if that is null will fallback to <see cref="ClaimTypes.NameIdentifier" />.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null" />
    /// </remarks>
    public string? UserIdClaimType { get; set; }

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true" />
    /// </remarks>
    public bool Discoverable { get; set; } = true;

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Preferred</c>
    /// </remarks>
    public string UserVerification { get; set; } = "Preferred";

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <c>None</c>
    /// </remarks>
    public string Attestation { get; set; } = "None";

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    // TODO: Should we have some sort of default lifetime? and is this used?
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="null" />
    /// </remarks>
    public string? AuthenticationType { get; set; }
}