namespace Passwordless.AspNetCore;

// TODO: We could allow customization for each endpoint through these options
public class PasswordlessEndpointOptions
{
    public string GroupPrefix { get; set; } = string.Empty;
    public string? RegisterPath { get; set; } = "/passwordless-register";
    public string? LoginPath { get; set; } = "/passwordless-login";
    public string? AddCredentialPath { get; set; } = "/passwordless-add-credential";
}