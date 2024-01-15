namespace Passwordless.Common.Models.Apps;

public record SetAppSettingsRequest(string PerformedBy, bool? EnableManuallyGeneratedAuthenticationTokens, bool? EnableMagicLinks);