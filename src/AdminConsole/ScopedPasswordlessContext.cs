namespace Passwordless.AdminConsole;

public interface IScopedPasswordlessContext
{
    string ApiSecret { get; set; }
}

public class ScopedPasswordlessContext : IScopedPasswordlessContext
{
    public string ApiSecret { get; set; } = string.Empty;
}