namespace Passwordless.Common.Services.IdGeneration;

public interface IIdGeneratorFactory
{
    IIdGenerator<T> Create<T>();
}