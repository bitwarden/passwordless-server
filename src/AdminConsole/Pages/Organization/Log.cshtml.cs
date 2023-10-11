using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Pages.Components;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Organization;

public class Log : PageModel
{
    private readonly IEventLogService _eventLogService;
    private readonly DataService _dataService;
    private readonly ICurrentContext _currentContext;

    public Models.Organization Organization { get; set; }
    public IEnumerable<EventLogEvent> Events { get; set; }
    public PagedList PageList { get; set; }
    public int RetentionPeriod { get; private set; }

    public Log(IEventLogService eventLogService, DataService dataService, ICurrentContext currentContext)
    {
        _eventLogService = eventLogService;
        _dataService = dataService;
        _currentContext = currentContext;
    }

    public async Task<ActionResult> OnGet(int pageNumber = 1, int numberOfResults = 100)
    {
        var features = _currentContext.OrganizationFeatures;

        if (!features.EventLoggingIsEnabled) return RedirectToPage("Overview");

        RetentionPeriod = features.EventLoggingRetentionPeriod;
        Organization = await _dataService.GetOrganization();

        // need to revisit this:
        // - do not pass around org id
        // - infinite scroll/paging over current approach?
        var result = await _eventLogService.GetEventLogs(Organization.Id, pageNumber, numberOfResults);
        Events = result.Events;
        var itemCount = await _eventLogService.GetEventLogCount(Organization.Id);

        PageList = new PagedList(itemCount, pageNumber, numberOfResults);

        return Page();
    }
}