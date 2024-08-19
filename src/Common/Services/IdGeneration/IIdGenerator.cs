namespace Passwordless.Common.Services.IdGeneration;

public interface IIdGenerator<T>
{
    T Generate();
}