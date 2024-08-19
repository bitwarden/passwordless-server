namespace Passwordless.Common.Services.IdGeneration;

public class IdGeneratorFactory : IIdGeneratorFactory
{
    public IIdGenerator<T> Create<T>()
    {
        if (typeof(T) == typeof(string))
        {
            return (IIdGenerator<T>)new StringIdGenerator();
        }

        throw new NotSupportedException($"No generator for type {typeof(T).Name}");
    }
}