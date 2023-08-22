using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Organization;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly DataService _dataService;

    [BindProperty]
    public Models.Organization Organization { get; set; }

    [BindProperty]
    public IEnumerable<AuditLogEvent> Events { get; set; }
    public Log(IAuditLogService auditLogService, DataService dataService)
    {
        _auditLogService = auditLogService;
        _dataService = dataService;
    }

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganization();
        Events = (await _auditLogService.GetAuditLogs(Organization.Id)).Events;
    }
}