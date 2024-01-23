namespace Passwordless.Common.Models.Apps;

/// <summary>
/// 
/// </summary>
/// <param name="IsAllowed">Whether or not an authenticator is whitelisted or blacklisted.</param>
public record ConfiguredAuthenticatorRequest(bool IsAllowed);