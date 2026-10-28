using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Passwordless.AdminConsole.Services;

/// <summary>
/// Stores the created secret key in TempData, which the default provider keeps in a data-protected cookie. The key
/// is scoped to the application it was created for and is removed as soon as it has been read.
/// </summary>
public class CreatedSecretKeyStore : ICreatedSecretKeyStore
{
    private const string KeyPrefix = "CreatedSecretKey:";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public CreatedSecretKeyStore(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public void Stash(string appId, string apiKey)
    {
        var tempData = GetTempData();
        tempData[KeyPrefix + appId] = apiKey;

        // Razor components don't save TempData automatically like MVC and Razor Pages do.
        tempData.Save();
    }

    public string? Take(string appId)
    {
        var tempData = GetTempData();
        var apiKey = tempData[KeyPrefix + appId] as string;

        // Reading marks the entry for deletion, saving persists that so the key can only be shown once.
        tempData.Save();

        return apiKey;
    }

    private ITempDataDictionary GetTempData() =>
        _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext!);
}