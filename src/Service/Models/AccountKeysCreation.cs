namespace Passwordless.Service.Models;

public class AccountKeysCreation
{
    public string? Message { get; set; }
    public required string ApiKey1 { get; set; }
    public required string ApiKey2 { get; set; }
    public required string ApiSecret1 { get; set; }
    public required string ApiSecret2 { get; set; }
}