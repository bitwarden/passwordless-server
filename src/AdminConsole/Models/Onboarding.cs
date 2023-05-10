namespace AdminConsole.Models;

public class Onboarding
{
    public string ApplicationId { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public DateTime SensitiveInfoExpireAt { get; set; }
}