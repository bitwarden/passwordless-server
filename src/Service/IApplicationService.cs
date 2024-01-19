using Passwordless.Common.Models.Apps;

namespace Passwordless.Service;

public interface IApplicationService
{
    Task SetFeaturesAsync(SetFeaturesRequest features);
}