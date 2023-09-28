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
    private readonly ICurrentContext _currentContext;
    private readonly ILogger<Log> _logger;

    public IEnumerable<AuditLogEvent> Events { get; private set; } = new List<AuditLogEvent>();
    public PagedList PageList { get; private set; }
    public int RetentionPeriod { get; private set; }

    public Log(IAuditLogService auditLogService, IPasswordlessManagementClient managementClient, ICurrentContext currentContext,
        ILogger<Log> logger)
    {
        _auditLogService = auditLogService;
        _currentContext = currentContext;
        _logger = logger;
    }

    public async Task<ActionResult> OnGet(int pageNumber = 1, int numberOfRecords = 100)
    {
        var features = _currentContext.Features;

        if (features.AuditLoggingIsEnabled == false) return Redirect("onboarding/get-started");

        RetentionPeriod = features.AuditLoggingRetentionPeriod;

        try
        {
            var eventsResponse = await _auditLogService.GetAuditLogs(pageNumber, numberOfRecords);

            Events = eventsResponse.Events;

            PageList = new PagedList(eventsResponse.TotalEventCount, pageNumber, numberOfRecords);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for {appId}", _currentContext.AppId);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }
    }
}