namespace Passwordless.Service.Storage.Ef;

public interface IStorageFactory
{
    IStorage Create();
}