using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Settings;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;

    public IEnumerable<AuditLogEvent> Events { get; set; } = new List<AuditLogEvent>();

    public Log(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task OnGet()
    {
        Events = (await _auditLogService.GetAuditLogs())?.Events ?? new List<AuditLogEvent>();
    }
}