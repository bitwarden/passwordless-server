namespace Passwordless.Api.Models;


public record AppIdDTO
{
    public string AppId { get; set; }
}

public record AppCreateDTO : AppIdDTO
{
    public string AdminEmail { get; set; } = "";
}

public class AccontCreateDTO
{
    public string AccountName { get; set; } = "";
    public string AdminEmail { get; set; } = "";
}

public class AppAvailable
{
    public required string AppId { get; set; }
}