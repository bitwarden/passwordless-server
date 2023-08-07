namespace Passwordless.Service.Storage.Ef.Global;

public interface IGlobalStorageFactory
{
    IGlobalStorage Create();
}