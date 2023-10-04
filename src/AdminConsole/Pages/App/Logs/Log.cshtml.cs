using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Pages.Components;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Logs;

public class Log : PageModel
{
    private readonly IEventLogService _eventLogService;
    private readonly ICurrentContext _currentContext;
    private readonly ILogger<Log> _logger;

    public IEnumerable<EventLogEvent> Events { get; private set; } = new List<EventLogEvent>();
    public PagedList PageList { get; private set; }
    public int RetentionPeriod { get; private set; }

    public Log(IEventLogService eventLogService, IPasswordlessManagementClient managementClient, ICurrentContext currentContext,
        ILogger<Log> logger)
    {
        _eventLogService = eventLogService;
        _currentContext = currentContext;
        _logger = logger;
    }

    public async Task<ActionResult> OnGet(int pageNumber = 1, int numberOfRecords = 100)
    {
        var features = _currentContext.Features;

        if (features.EventLoggingIsEnabled == false) return Redirect("onboarding/get-started");

        RetentionPeriod = features.EventLoggingRetentionPeriod;

        try
        {
            var eventsResponse = await _eventLogService.GetEventLogs(pageNumber, numberOfRecords);

            Events = eventsResponse.Events;

            PageList = new PagedList(eventsResponse.TotalEventCount, pageNumber, numberOfRecords);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event logs for {appId}", _currentContext.AppId);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }
    }
}