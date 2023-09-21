using Passwordless.Service.Models;

namespace Passwordless.Service;

public interface IApplicationService
{
    Task SetFeaturesAsync(SetFeaturesDto features);
}