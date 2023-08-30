using AdminConsole.Pages.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Settings;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;

    public IEnumerable<AuditLogEvent> Events { get; set; } = new List<AuditLogEvent>();
    public PagedList PageList { get; set; }

    public Log(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task OnGet(int pageNumber = 1, int numberOfRecords = 100)
    {
        var eventsResponse = await _auditLogService.GetAuditLogs(pageNumber, numberOfRecords);

        Events = eventsResponse.Events;

        PageList = new PagedList(eventsResponse.TotalEventCount, pageNumber, numberOfRecords);
    }
}