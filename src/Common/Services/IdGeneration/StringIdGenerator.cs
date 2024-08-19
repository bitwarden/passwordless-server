namespace Passwordless.Common.Services.IdGeneration;

public class StringIdGenerator : IIdGenerator<string>
{
    public string Generate()
    {
        return Guid.NewGuid().ToString("N");
    }
}