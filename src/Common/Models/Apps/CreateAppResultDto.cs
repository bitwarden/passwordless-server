namespace Passwordless.Common.Models.Apps;

public class CreateAppResultDto
{
    public string Message { get; set; }
    public string ApiKey1 { get; set; }
    public string ApiKey2 { get; set; }
    public string ApiSecret1 { get; set; }
    public string ApiSecret2 { get; set; }
}