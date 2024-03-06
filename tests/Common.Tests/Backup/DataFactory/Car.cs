namespace Passwordless.Common.Tests.Backup.DataFactory;

public class Car
{
    public int Id { get; set; }

    public string Make { get; set; }

    public int OwnerId { get; set; }

    public Person Owner { get; set; }
}