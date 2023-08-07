namespace Passwordless.Service.Storage.Ef;

public interface IGlobalStorageFactory
{
    IGlobalStorage Create();
}