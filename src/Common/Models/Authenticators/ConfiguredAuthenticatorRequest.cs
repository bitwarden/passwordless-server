namespace Passwordless.Common.Models.Authenticators;

/// <summary>
/// 
/// </summary>
/// <param name="IsAllowed">When 'true', all authenticators on the allowlist are returned. When 'false', all authenticators on the blocklist are returned.</param>
public record ConfiguredAuthenticatorRequest(bool IsAllowed);