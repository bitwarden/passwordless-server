using AdminConsole.Pages.Components;
using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Organization;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly DataService _dataService;

    public Models.Organization Organization { get; set; }

    public IEnumerable<AuditLogEvent> Events { get; set; }
    public PagedList PageList { get; set; }
    
    public Log(IAuditLogService auditLogService, DataService dataService)
    {
        _auditLogService = auditLogService;
        _dataService = dataService;
    }

    public async Task OnGet(int pageNumber = 1, int numberOfResults = 100)
    {
        Organization = await _dataService.GetOrganization();
        Events = (await _auditLogService.GetAuditLogs(Organization.Id, pageNumber, numberOfResults)).Events;

        var itemCount = await _auditLogService.GetAuditLogCount(Organization.Id);
        
        PageList = new PagedList(itemCount, pageNumber, numberOfResults);
    }
}