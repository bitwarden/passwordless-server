using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Loggers;

public interface IEventLogStorage
{
    Task<IEnumerable<ApplicationEvent>> GetEventLogAsync(int pageNumber, int resultsPerPage, CancellationToken cancellationToken);
    Task<int> GetEventLogCountAsync(CancellationToken cancellationToken);
}