using AdminConsole.Pages.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Logs;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ICurrentContext _currentContext;

    public IEnumerable<AuditLogEvent> Events { get; private set; } = new List<AuditLogEvent>();
    public PagedList PageList { get; private set; }
    public int RetentionPeriod { get; private set; }

    public Log(IAuditLogService auditLogService, IPasswordlessManagementClient managementClient, ICurrentContext currentContext)
    {
        _auditLogService = auditLogService;
        _managementClient = managementClient;
        _currentContext = currentContext;
    }

    public async Task<ActionResult> OnGet(int pageNumber = 1, int numberOfRecords = 100)
    {
        var features = _currentContext.Features;

        if (features.AuditLoggingIsEnabled == false) return Redirect("onboarding/get-started");

        RetentionPeriod = features.AuditLoggingRetentionPeriod;

        var eventsResponse = await _auditLogService.GetAuditLogs(pageNumber, numberOfRecords);

        Events = eventsResponse.Events;

        PageList = new PagedList(eventsResponse.TotalEventCount, pageNumber, numberOfRecords);

        return Page();
    }
}